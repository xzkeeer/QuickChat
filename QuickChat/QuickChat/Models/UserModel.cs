using System.ComponentModel;

namespace QuickChat.Models
{
    public class UserModel : INotifyPropertyChanged
    {
        private string _avatarUrl;
        public int Id { get; set; }
        public string Username { get; set; }
        public bool HasExistingChat { get; set; }
        public string AvatarUrl
        {
            get => _avatarUrl;
            set
            {
                if (_avatarUrl != value)
                {
                    _avatarUrl = value;
                    OnPropertyChanged(nameof(AvatarUrl));
                }
            }
        }
        public bool IsOnline { get; set; }
        public DateTime? LastOnline { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
