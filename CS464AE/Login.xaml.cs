using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CS464AE.Data;

namespace CS464AE
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private Repository _repository;

        public event EventHandler<LoginSuccessfulEventArgs> LoginSuccessful;

        public Login()
        {
            InitializeComponent();
            _repository = new Repository();
            
            // Thiết lập tab mặc định
            tabControl.SelectedIndex = 0;
            
            // Thêm event handler cho nút chính
            btnMainAction.Click += BtnMainAction_Click;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Cập nhật nút dựa trên tab được chọn
            if (tabControl.SelectedIndex == 0) // Tab Login
            {
                btnMainAction.Content = "🔑 Đăng nhập";
                btnMainAction.Background = new SolidColorBrush(Color.FromRgb(46, 134, 171)); // #2E86AB
            }
            else // Tab Register
            {
                btnMainAction.Content = "📝 Đăng ký";
                btnMainAction.Background = new SolidColorBrush(Color.FromRgb(162, 59, 114)); // #A23B72
            }
        }

        private void BtnMainAction_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl.SelectedIndex == 0) // Tab Login
            {
                PerformLogin();
            }
            else // Tab Register
            {
                PerformRegister();
            }
        }

        private void PerformLogin()
        {
            try
            {
                string username = txtLoginUsername.Text.Trim();
                string password = txtLoginPassword.Password;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ShowError("Vui lòng nhập đầy đủ tên tài khoản và mật khẩu!");
                    return;
                }

                var user = _repository.Login(username, password);
                if (user != null)
                {
                    // Đăng nhập thành công
                    LoginSuccessful?.Invoke(this, new LoginSuccessfulEventArgs(user.Id, user.TenDangNhap));
                    this.Close();
                }
                else
                {
                    ShowError("Tên tài khoản hoặc mật khẩu không đúng!");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi đăng nhập: {ex.Message}");
            }
        }

        private void PerformRegister()
        {
            try
            {
                string username = txtRegisterUsername.Text.Trim();
                string password = txtRegisterPassword.Password;
                string phone = txtRegisterPhone.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ShowError("Vui lòng nhập đầy đủ tên tài khoản và mật khẩu!");
                    return;
                }

                if (password.Length < 6)
                {
                    ShowError("Mật khẩu phải có ít nhất 6 ký tự!");
                    return;
                }

                // Validation cho số điện thoại (nếu có)
                if (!string.IsNullOrEmpty(phone) && (phone.Length < 10 || phone.Length > 15))
                {
                    ShowError("Số điện thoại phải có từ 10-15 ký tự!");
                    return;
                }

                // Kiểm tra username đã tồn tại chưa
                if (_repository.CheckUsernameExists(username))
                {
                    ShowError("Tên tài khoản đã tồn tại!");
                    return;
                }

                var newUser = new User
                {
                    TenDangNhap = username,
                    MatKhau = password,
                    SoDienThoai = string.IsNullOrEmpty(phone) ? null : phone,
                    NgayTao = DateTime.Now
                };

                try
                {
                    if (_repository.Register(newUser))
                    {
                        ShowSuccess("Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.");
                        // Chuyển sang tab Login
                        tabControl.SelectedIndex = 0;
                        txtLoginUsername.Text = username;
                        txtLoginPassword.Password = password;
                        // Clear form đăng ký
                        ClearRegisterForm();
                    }
                    else
                    {
                        ShowError("Đăng ký thất bại! Tên tài khoản có thể đã tồn tại.");
                    }
                }
                catch (Exception dbEx)
                {
                    ShowError($"Lỗi database: {dbEx.Message}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi đăng ký: {ex.Message}");
            }
        }


        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowError(string message)
        {
            txtErrorMessage.Text = message;
            errorBorder.Visibility = Visibility.Visible;
            txtStatus.Text = "Lỗi";
        }

        private void ShowSuccess(string message)
        {
            txtErrorMessage.Text = message;
            txtErrorMessage.Foreground = new SolidColorBrush(Colors.Green);
            errorBorder.Background = new SolidColorBrush(Color.FromRgb(230, 255, 230));
            errorBorder.BorderBrush = new SolidColorBrush(Colors.Green);
            errorBorder.Visibility = Visibility.Visible;
            txtStatus.Text = "Thành công";
        }

        private void ClearRegisterForm()
        {
            txtRegisterUsername.Text = "";
            txtRegisterPassword.Password = "";
            txtRegisterPhone.Text = "";
        }

        private void ClearErrorMessage()
        {
            errorBorder.Visibility = Visibility.Collapsed;
            txtStatus.Text = "Sẵn sàng";
        }

        protected override void OnClosed(EventArgs e)
        {
            _repository?.Dispose();
            base.OnClosed(e);
        }

     
    }
}
