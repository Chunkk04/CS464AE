using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CS464AE.Data;

namespace CS464AE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Repository _repository;
        private List<TodoItem> _allTodos;
        private List<TodoItem> _currentTodos;
        private int _currentUserId = 0; // Chưa đăng nhập
        private string _currentView = "Tất cả công việc";

        public MainWindow()
        {
            InitializeComponent();
            _repository = new Repository();
            
            // Ẩn nút logout ban đầu
            btnLogout.Visibility = Visibility.Collapsed;
            
            LoadTodos();
            UpdateStatistics();
        }

        private void LoadTodos()
        {
            try
            {
                if (_currentUserId > 0)
                {
                    _allTodos = _repository.GetTodosByUser(_currentUserId);
                    _currentTodos = _allTodos;
                    lvTodos.ItemsSource = _currentTodos;
                    txtCurrentView.Text = _currentView;
                    UpdateStatistics();
                }
                else
                {
                    // Chưa đăng nhập - hiển thị danh sách rỗng
                    _allTodos = new List<TodoItem>();
                    _currentTodos = _allTodos;
                    lvTodos.ItemsSource = _currentTodos;
                    txtCurrentView.Text = "Vui lòng đăng nhập";
                    
                    // Reset statistics
                    txtTotalTodos.Text = "Tổng: 0";
                    txtPendingTodos.Text = "Chưa xong: 0";
                    txtCompletedTodos.Text = "Đã xong: 0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                if (_currentUserId > 0)
                {
                    int total = _repository.GetTotalTodosCount(_currentUserId);
                    int pending = _repository.GetPendingTodosCount(_currentUserId);
                    int completed = _repository.GetCompletedTodosCount(_currentUserId);

                    txtTotalTodos.Text = $"Tổng: {total}";
                    txtPendingTodos.Text = $"Chưa xong: {pending}";
                    txtCompletedTodos.Text = $"Đã xong: {completed}";
                }
                else
                {
                    txtTotalTodos.Text = "Tổng: 0";
                    txtPendingTodos.Text = "Chưa xong: 0";
                    txtCompletedTodos.Text = "Đã xong: 0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thống kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserId <= 0)
            {
                MessageBox.Show("Vui lòng đăng nhập để thêm công việc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var todoForm = new TodoForm(_currentUserId);
            todoForm.Owner = this;
            todoForm.TodoSaved += OnTodoSaved;
            todoForm.ShowDialog();
        }

        private void BtnEditTodo_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserId <= 0)
            {
                MessageBox.Show("Vui lòng đăng nhập để sửa công việc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = lvTodos.SelectedItem as TodoItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một công việc để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var todoForm = new TodoForm(_currentUserId, selectedItem);
            todoForm.Owner = this;
            todoForm.TodoSaved += OnTodoSaved;
            todoForm.ShowDialog();
        }

        private void BtnDeleteTodo_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = lvTodos.SelectedItem as TodoItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một công việc để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc muốn xóa '{selectedItem.Title}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_repository.DeleteTodo(selectedItem.Id))
                    {
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTodos(); // Reload danh sách
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa công việc này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new Login();
            loginWindow.LoginSuccessful += OnLoginSuccessful;
            loginWindow.ShowDialog();
        }

        private void OnLoginSuccessful(object sender, LoginSuccessfulEventArgs e)
        {
            _currentUserId = e.UserId;
            LoadTodos();
            UpdateStatistics();
            txtStatus.Text = $"Đã đăng nhập: {e.Username}";
            
            // Cập nhật UI sau khi đăng nhập
            btnLogin.Content = "👤 " + e.Username;
            btnLogout.Visibility = Visibility.Visible;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            _currentUserId = 0; // Reset user ID
            LoadTodos(); // Clear todos
            UpdateStatistics(); // Reset statistics
            txtStatus.Text = "Đã đăng xuất";
            
            // Reset UI
            btnLogin.Content = "Đăng nhập";
            btnLogout.Visibility = Visibility.Collapsed;
        }

        private void BtnAllTodos_Click(object sender, RoutedEventArgs e)
        {
            _currentTodos = _allTodos;
            lvTodos.ItemsSource = _currentTodos;
            _currentView = "Tất cả công việc";
            txtCurrentView.Text = _currentView;
        }

        private void BtnPendingTodos_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserId <= 0)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem công việc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _currentTodos = _repository.GetPendingTodos(_currentUserId);
                lvTodos.ItemsSource = _currentTodos;
                _currentView = "Chưa hoàn thành";
                txtCurrentView.Text = _currentView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCompletedTodos_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserId <= 0)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem công việc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _currentTodos = _repository.GetCompletedTodos(_currentUserId);
                lvTodos.ItemsSource = _currentTodos;
                _currentView = "Đã hoàn thành";
                txtCurrentView.Text = _currentView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOverdueTodos_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserId <= 0)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem công việc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _currentTodos = _repository.GetOverdueTodos(_currentUserId);
                lvTodos.ItemsSource = _currentTodos;
                _currentView = "Quá hạn";
                txtCurrentView.Text = _currentView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnStatistics_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thống kê chi tiết sẽ được implement sau!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }





        private void LvTodos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Có thể thêm logic xử lý khi chọn item
            var selectedItem = lvTodos.SelectedItem as TodoItem;
            if (selectedItem != null)
            {
                txtStatus.Text = $"Đã chọn: {selectedItem.Title}";
            }
        }

        private void OnTodoSaved(object sender, TodoSavedEventArgs e)
        {
            LoadTodos(); // Reload danh sách sau khi thêm/sửa
        }

        // Thêm method để dispose repository khi đóng window
        protected override void OnClosed(EventArgs e)
        {
            _repository?.Dispose();
            base.OnClosed(e);
        }
    }
}
