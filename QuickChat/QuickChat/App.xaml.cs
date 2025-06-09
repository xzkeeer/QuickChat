using QuickChat.Models;
using QuickChat.Services;
using QuickChat.ViewModels;
using System.Windows;

namespace QuickChat
{
    public partial class App : Application
    {
        public UserModel CurrentUser { get; set; }
        public ApiClient ApiClient { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ApiClient = new ApiClient("https://localhost:7271");

            // Показываем окно авторизации
            var authWindow = new Authorization();
            authWindow.DataContext = new AuthViewModel(ApiClient, new AuthService(ApiClient));
            authWindow.Show();
           
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
    }
}