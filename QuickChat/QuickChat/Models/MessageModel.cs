using QuickChat.Models;
using System.Windows.Media;
using System.Windows;

public class MessageModel
{
    private bool _isRead;
    public int Id { get; set; }
    public string Text { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public int SenderId { get; set; }
    public int ChatId { get; set; }

    public UserModel Sender { get; set; }
    public int CurrentUserId { get; set; } 

    public HorizontalAlignment MessageAlignment =>
        SenderId == CurrentUserId ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    public Brush BubbleColor =>
        SenderId == CurrentUserId ? Brushes.LightBlue : Brushes.LightGray;

    public bool IsIncoming => SenderId != CurrentUserId;

    public string FormattedTime => SentAt.AddHours(3).ToString("HH:mm");
    public Visibility ReadStatusVisibility =>
       (SenderId == CurrentUserId && !IsIncoming) ? Visibility.Visible : Visibility.Collapsed;

    public string ReadStatusIcon =>
        IsRead ? "✓✓" : "✓"; 

    public Brush ReadStatusColor =>
        IsRead ? Brushes.Blue : Brushes.Gray;
}