using System;
using System.Linq;
using System.Windows;

namespace CS464AE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Đảm bảo DataDirectory được set đúng - trỏ về thư mục project
            string projectPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            System.AppDomain.CurrentDomain.SetData("DataDirectory", projectPath);
            
            // Kiểm tra và tạo database nếu cần
            try
            {
                using (var context = new DatabaseEntities())
                {
                    // Thử kết nối để đảm bảo database tồn tại
                    var count = context.Users.Count();
                    System.Diagnostics.Debug.WriteLine($"Database connection successful. Users count: {count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection failed: {ex.Message}");
                // Nếu không kết nối được, thử tạo database
                try
                {
                    using (var context = new DatabaseEntities())
                    {
                        context.Database.CreateIfNotExists();
                        System.Diagnostics.Debug.WriteLine("Database created successfully");
                    }
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create database: {ex2.Message}");
                }
            }
        }
    }
}
