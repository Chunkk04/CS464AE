using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace CS464AE.Data
{
    public class Repository
    {
        private DatabaseEntities _context;

        public Repository()
        {
            _context = new DatabaseEntities();
        }

        // User operations
        public User Login(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.TenDangNhap == username && u.MatKhau == password);
        }

        public bool Register(User user)
        {
            try
            {
                // Kiểm tra username đã tồn tại chưa
                if (_context.Users.Any(u => u.TenDangNhap == user.TenDangNhap))
                {
                    return false;
                }

                _context.Users.Add(user);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
                throw; // Re-throw để có thể catch ở UI layer
            }
        }

        public bool CheckUsernameExists(string username)
        {
            return _context.Users.Any(u => u.TenDangNhap == username);
        }

        // Todo operations
        public List<TodoItem> GetTodosByUser(int userId)
        {
            return _context.TodoItems.Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedDate).ToList();
        }

        public List<TodoItem> GetPendingTodos(int userId)
        {
            return _context.TodoItems.Where(t => t.UserId == userId && !t.IsCompleted).ToList();
        }

        public List<TodoItem> GetCompletedTodos(int userId)
        {
            return _context.TodoItems.Where(t => t.UserId == userId && t.IsCompleted).ToList();
        }

        public List<TodoItem> GetOverdueTodos(int userId)
        {
            return _context.TodoItems.Where(t => t.UserId == userId && !t.IsCompleted && t.DueDate < DateTime.Now).ToList();
        }

        public bool AddTodo(TodoItem todo)
        {
            try
            {
                _context.TodoItems.Add(todo);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateTodo(TodoItem todo)
        {
            try
            {
                _context.Entry(todo).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteTodo(int todoId)
        {
            try
            {
                var todo = _context.TodoItems.Find(todoId);
                if (todo != null)
                {
                    _context.TodoItems.Remove(todo);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public TodoItem GetTodoById(int todoId)
        {
            return _context.TodoItems.Find(todoId);
        }

        // Statistics
        public int GetTotalTodosCount(int userId)
        {
            return _context.TodoItems.Count(t => t.UserId == userId);
        }

        public int GetPendingTodosCount(int userId)
        {
            return _context.TodoItems.Count(t => t.UserId == userId && !t.IsCompleted);
        }

        public int GetCompletedTodosCount(int userId)
        {
            return _context.TodoItems.Count(t => t.UserId == userId && t.IsCompleted);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
