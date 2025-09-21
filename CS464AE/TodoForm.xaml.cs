using System;
using System.Windows;
using CS464AE.Data;

namespace CS464AE
{
    /// <summary>
    /// Interaction logic for TodoForm.xaml
    /// </summary>
    public partial class TodoForm : Window
    {
        private Repository _repository;
        private TodoItem _currentTodo;
        private int _userId;
        private bool _isEditMode;

        public event EventHandler<TodoSavedEventArgs> TodoSaved;

        public TodoForm(int userId, TodoItem todoItem = null)
        {
            InitializeComponent();
            _repository = new Repository();
            _userId = userId;
            _currentTodo = todoItem;
            _isEditMode = todoItem != null;

            InitializeForm();
        }

        private void InitializeForm()
        {
            if (_isEditMode)
            {
                // Chế độ sửa
                txtHeader.Text = "✏️ Sửa công việc";
                txtTitle.Text = _currentTodo.Title;
                txtDescription.Text = _currentTodo.Description;
                cmbCategory.Text = _currentTodo.Category;
                dpDueDate.SelectedDate = _currentTodo.DueDate;
                chkCompleted.IsChecked = _currentTodo.IsCompleted;
                pnlStatus.Visibility = Visibility.Visible;
                btnSave.Content = "💾 Cập nhật";
            }
            else
            {
                // Chế độ thêm mới
                txtHeader.Text = "➕ Thêm công việc mới";
                dpDueDate.SelectedDate = DateTime.Now.AddDays(7); // Mặc định 7 ngày
                cmbCategory.SelectedIndex = 0; // Chọn "Học tập" mặc định
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    ShowError("Vui lòng nhập tiêu đề công việc!");
                    txtTitle.Focus();
                    return;
                }

                if (dpDueDate.SelectedDate == null)
                {
                    ShowError("Vui lòng chọn hạn chót!");
                    dpDueDate.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(cmbCategory.Text))
                {
                    ShowError("Vui lòng chọn danh mục!");
                    cmbCategory.Focus();
                    return;
                }

                // Tạo hoặc cập nhật TodoItem
                if (_isEditMode)
                {
                    // Cập nhật
                    _currentTodo.Title = txtTitle.Text.Trim();
                    _currentTodo.Description = txtDescription.Text.Trim();
                    _currentTodo.Category = cmbCategory.Text.Trim();
                    _currentTodo.DueDate = dpDueDate.SelectedDate.Value;
                    _currentTodo.IsCompleted = chkCompleted.IsChecked ?? false;
                    
                    if (_currentTodo.IsCompleted && _currentTodo.CompletedDate == null)
                    {
                        _currentTodo.CompletedDate = DateTime.Now;
                    }
                    else if (!_currentTodo.IsCompleted)
                    {
                        _currentTodo.CompletedDate = null;
                    }

                    if (_repository.UpdateTodo(_currentTodo))
                    {
                        ShowSuccess("Cập nhật công việc thành công!");
                        TodoSaved?.Invoke(this, new TodoSavedEventArgs(_currentTodo));
                        this.Close();
                    }
                    else
                    {
                        ShowError("Không thể cập nhật công việc!");
                    }
                }
                else
                {
                    // Thêm mới
                    var newTodo = new TodoItem
                    {
                        Title = txtTitle.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        Category = cmbCategory.Text.Trim(),
                        DueDate = dpDueDate.SelectedDate.Value,
                        CreatedDate = DateTime.Now,
                        UserId = _userId,
                        IsCompleted = false
                    };

                    if (_repository.AddTodo(newTodo))
                    {
                        ShowSuccess("Thêm công việc mới thành công!");
                        TodoSaved?.Invoke(this, new TodoSavedEventArgs(newTodo));
                        this.Close();
                    }
                    else
                    {
                        ShowError("Không thể thêm công việc mới!");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowError(string message)
        {
            txtErrorMessage.Text = message;
            txtErrorMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            errorBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 230, 230));
            errorBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            errorBorder.Visibility = Visibility.Visible;
            txtStatus.Text = "Lỗi";
        }

        private void ShowSuccess(string message)
        {
            txtErrorMessage.Text = message;
            txtErrorMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            errorBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 255, 230));
            errorBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            errorBorder.Visibility = Visibility.Visible;
            txtStatus.Text = "Thành công";
        }

        protected override void OnClosed(EventArgs e)
        {
            _repository?.Dispose();
            base.OnClosed(e);
        }
    }

    public class TodoSavedEventArgs : EventArgs
    {
        public TodoItem TodoItem { get; }

        public TodoSavedEventArgs(TodoItem todoItem)
        {
            TodoItem = todoItem;
        }
    }
}
