using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuickChat;
using QuickChat.Models;
using QuickChat.Services;
using QuickChat.ViewModels;
public class MainViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private ChatModel _selectedChat;
    private string _messageText;
    private UserModel _currentUser;
    private ScrollViewer _messagesScrollViewer;
    private string _searchQuery;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            SetProperty(ref _searchQuery, value);
            SearchUsers();
        }
    }
    public UserModel CurrentUser
    {
        get => _currentUser;
        set
        {
            SetProperty(ref _currentUser, value);
            LoadChats();
        }
    }
    public ObservableCollection<ChatModel> Chats { get; } = new();
    public ObservableCollection<MessageModel> Messages { get; } = new();
    private ObservableCollection<UserModel> _searchResults = new();
    public ObservableCollection<UserModel> SearchResults
    {
        get => _searchResults;
        set => SetProperty(ref _searchResults, value);
    }
    public ChatModel SelectedChat
    {
        get => _selectedChat;
        set
        {
            SetProperty(ref _selectedChat, value);
            if (value != null) LoadMessages(value.Id);
        }
    }
    public string MessageText
    {
        get => _messageText;
        set => SetProperty(ref _messageText, value);
    }

    public ICommand SendMessageCommand { get; }
    public ICommand StartNewChatCommand { get; }
    public ICommand ChangeAvatarCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    private bool _isSettingsOpen;
    public bool IsSettingsOpen
    {
        get => _isSettingsOpen;
        set => SetProperty(ref _isSettingsOpen, value);
    }
    public MainViewModel(ApiClient apiClient, UserModel currentUser)
    {
        _apiClient = apiClient;
        CurrentUser = currentUser;
        SendMessageCommand = new RelayCommand(async () => await SendMessage(),
            () => !string.IsNullOrWhiteSpace(MessageText) && SelectedChat != null);
        StartNewChatCommand = new RelayCommand(async () => await StartNewChat());
        ChangeAvatarCommand = new RelayCommand(async () => await ChangeAvatar());
        LogoutCommand = new RelayCommand(Logout);
        OpenSettingsCommand = new RelayCommand(() => IsSettingsOpen = !IsSettingsOpen);
       
    }
    private async Task ChangeAvatar()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };
        if (openFileDialog.ShowDialog() == true)
        {
            var imageBytes = File.ReadAllBytes(openFileDialog.FileName);
            var base64String = Convert.ToBase64String(imageBytes);
            var mimeType = "image/jpeg";
            if (openFileDialog.FileName.EndsWith(".png")) mimeType = "image/png";
            else if (openFileDialog.FileName.EndsWith(".gif")) mimeType = "image/gif";

            var avatarData = $"data:{mimeType};base64,{base64String}";

            var updatedUser = await _apiClient.PutAsync<UserModel>("api/users/avatar", new
            {
                UserId = CurrentUser.Id,
                AvatarBase64 = avatarData
            });
            if (updatedUser != null)
            {
                CurrentUser.AvatarUrl = updatedUser.AvatarUrl;
                OnPropertyChanged(nameof(CurrentUser));
                foreach (var chat in Chats)
                {
                    var participant = chat.Participants?.FirstOrDefault(p => p.Id == CurrentUser.Id);
                    if (participant != null)
                    {
                        participant.AvatarUrl = updatedUser.AvatarUrl;
                    }
                }
                if (SelectedChat != null)
                {
                    LoadMessages(SelectedChat.Id);
                }
                CurrentUser.AvatarUrl = $"{updatedUser.AvatarUrl}?t={DateTime.Now.Ticks}";
                OnPropertyChanged(nameof(CurrentUser));
            }
        }
    }
    public async Task StartChatWithUser(UserModel user)
    {
        try
        {
            var newChat = await _apiClient.PostAsync<ChatModel>("api/chats", new
            {
                Name = $"Чат с {user.Username}",
                IsGroup = false,
                ParticipantIds = new[] { CurrentUser.Id, user.Id }
            });

            if (newChat != null)
            {
                // Обновляем список чатов
                LoadChats();

                // Выбираем новый чат
                SelectedChat = Chats.FirstOrDefault(c => c.Id == newChat.Id);

                // Очищаем поиск
                SearchQuery = string.Empty;
                SearchResults.Clear();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating chat: {ex}");
            MessageBox.Show("Не удалось создать чат", "Ошибка",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private async Task SearchUsers()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        try
        {
            var users = await _apiClient.GetAsync<List<UserModel>>($"api/users/search?query={SearchQuery}");

            // Проверяем существующие чаты
            foreach (var user in users)
            {
                user.HasExistingChat = this.Chats.Any(c =>
                    !c.IsGroup &&
                    c.Participants != null &&
                    c.Participants.Any(p => p.Id == user.Id));
            }

            SearchResults = new ObservableCollection<UserModel>(users);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Search error: {ex}");
        }
    }
    private void Logout()
    {
        var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Application.Current.MainWindow?.Close();
            var authWindow = new Authorization();

            Application.Current.MainWindow = authWindow;

            authWindow.Show();
        }
    }
    public void SetMessagesScrollViewer(ScrollViewer scrollViewer)
    {
        _messagesScrollViewer = scrollViewer;
    }
    private async void LoadChats()
    {
        var chats = await _apiClient.GetAsync<List<ChatModel>>($"api/chats?userId={CurrentUser.Id}");
        Application.Current.Dispatcher.Invoke(() =>
        {
            Chats.Clear();
            foreach (var chat in chats)
            {
                if (!chat.IsGroup && chat.Participants?.Count == 2)
                {
                    var otherUser = chat.Participants.FirstOrDefault(p => p.Id != CurrentUser.Id);
                    chat.Name = otherUser?.Username ?? "Private Chat";
                    chat.Avatar = otherUser?.AvatarUrl ?? "default_avatar.png";
                    chat.IsOnline = otherUser?.IsOnline ?? false;
                }
                Chats.Add(chat);
            }
        });
    }
    private async void LoadMessages(int chatId)
    {
        try
        {
            var messages = await _apiClient.GetAsync<List<MessageModel>>($"api/messages?chatId={chatId}");
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Clear();
                foreach (var msg in messages)
                {
                    msg.CurrentUserId = CurrentUser.Id; 
                    Messages.Add(msg);
                }
                _messagesScrollViewer?.ScrollToEnd();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки сообщений: {ex}");
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || SelectedChat == null) return;
        {
            var response = await _apiClient.PostAsync<MessageModel>("api/messages", new
            {
                Text = MessageText,
                ChatId = SelectedChat.Id,
                SenderId = CurrentUser.Id
            });

            if (response != null)
            {
                MessageText = string.Empty;
                LoadMessages(SelectedChat.Id);

                await Task.Delay(100);
                _messagesScrollViewer?.ScrollToEnd();
            }
        }
    }
    private async Task StartNewChat()
    {
        var dialog = new InputDialog("Новый чат", "Введите имя пользователя:");
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.Answer)) return;

        string username = dialog.Answer;

        var user = await _apiClient.GetAsync<UserModel>($"api/users/by-username/{username}");
        if (user == null)
        {
            MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var newChat = await _apiClient.PostAsync<ChatModel>("api/chats", new
        {
            Name = $"Чат с {user.Username}",
            IsGroup = false,
            ParticipantIds = new[] { CurrentUser.Id, user.Id }
        });
        if (newChat != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Chats.Add(newChat);
                SelectedChat = newChat;
                LoadChats();
            });
        }
    }
}