using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QuickChat.Models;
using QuickChat.Services;
using QuickChat.ViewModels;

namespace QuickChat
{
    public partial class MainWindow : Window
    {
        private ItemsControl _messagesItemsControl;
        private readonly Window _window;
        public MainWindow(UserModel user)
        {
            InitializeComponent();

            var apiClient = new ApiClient("https://localhost:7271");
            DataContext = new MainViewModel(apiClient, user);

            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Находим ItemsControl для сообщений
            _messagesItemsControl = FindName("MessagesItemsControl") as ItemsControl;

            if (DataContext is MainViewModel viewModel && _messagesItemsControl != null)
            {
                if (VisualTreeHelper.GetChild(_messagesItemsControl, 0) is Decorator border &&
                    border.Child is ScrollViewer scrollViewer)
                {
                    viewModel.SetMessagesScrollViewer(scrollViewer);
                }
            }
        }
        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T result)
                    return result;
                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }
        private void SearchResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel viewModel &&
                ((ListView)sender).SelectedItem is UserModel selectedUser)
            {
                if (selectedUser.HasExistingChat)
                {
                    // Переходим в существующий чат
                    var existingChat = viewModel.Chats.FirstOrDefault(c =>
                        !c.IsGroup &&
                        c.Participants != null &&
                        c.Participants.Any(p => p.Id == selectedUser.Id));

                    if (existingChat != null)
                    {
                        viewModel.SelectedChat = existingChat;
                       
                    }
                }
                else
                {
                    // Создаем новый чат
                    viewModel.StartChatWithUser(selectedUser);
                }
            }
        }

    }
}