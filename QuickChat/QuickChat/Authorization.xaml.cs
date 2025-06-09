using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QuickChat.Models;
using QuickChat.Services;
using QuickChat.ViewModels;

namespace QuickChat
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
            var apiClient = new ApiClient("https://localhost:7271");
            var authService = new AuthService(apiClient);
            DataContext = new AuthViewModel(apiClient, authService);
          
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            LoginPasswordBox.PasswordChanged += LoginPassword_PasswordChanged;

        }
       

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthViewModel vm)
            {
                vm.RegisterCommand.Execute(null);
            }
        }

        private void SelectAvatar_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthViewModel vm)
            {
                vm.SelectAvatarCommand.Execute(null);
            }

        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthViewModel vm)
            {
                vm.LoginCommand.Execute(null);
            }
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }
        private void LoginPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
            
        }

    }
}
