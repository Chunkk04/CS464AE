using System;
using System.Windows;

namespace CS464AE
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
