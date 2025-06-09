using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using QuickChat.Models;
using QuickChat.Services;

namespace QuickChat.ViewModels
{
    public class AuthViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly ApiClient _apiClient;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _name = string.Empty;
        private string _avatarUrl = "default_avatar.png";
        private bool _isBusy;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value?.Trim());
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand SelectAvatarCommand { get; }
        public AuthViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;

            LoginCommand = new RelayCommand(async () => await Login(),
                () => !IsBusy && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));

            RegisterCommand = new RelayCommand(async () => await Register(),
                () => !IsBusy && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));

            SelectAvatarCommand = new RelayCommand(SelectAvatar);
        }
        private async Task Login()
        {
            var loginData = new
            {
                username = Username.ToLower(),
                password = Password
            };

            Debug.WriteLine($"Login attempt: {JsonSerializer.Serialize(loginData)}");

            var user = await _authService.Login(loginData);

            if (user != null && user.Username != "error")
            {
                OpenMainWindow(user);
            }

        }

        private async Task Register()
        {
            try
            {
                IsBusy = true;

                // Читаем файл аватара и конвертируем в Base64
                string avatarBase64 = null;
                if (!string.IsNullOrEmpty(AvatarUrl) && File.Exists(AvatarUrl))
                {
                    var bytes = File.ReadAllBytes(AvatarUrl);
                    var mimeType = GetMimeType(AvatarUrl);
                    avatarBase64 = $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";
                }

                var registerData = new
                {
                    username = Username.ToLower(),
                    password = Password,
                    name = Name,
                    avatarBase64 = avatarBase64 // Отправляем Base64 вместо пути
                };

                Debug.WriteLine($"Registration attempt: {JsonSerializer.Serialize(registerData)}");

                await _authService.Register(registerData);
                MessageBox.Show("Регистрация успешна!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration error: {ex}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
        private void OpenMainWindow(UserModel user)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var apiClient = new ApiClient("https://localhost:7271");
                var mainWindow = new MainWindow(user)
                {
                    DataContext = new MainViewModel(apiClient, user)
                };
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Authorization authWindow && window != mainWindow)
                    {
                        authWindow.Close();
                        break;
                    }
                }
            });
        }
        private void SelectAvatar()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AvatarUrl = openFileDialog.FileName;
            }
        }
    }
}
