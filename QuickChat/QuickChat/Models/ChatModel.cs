using QuickChat.Models;

public class ChatModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Avatar { get; set; }
    public string LastMessagePreview { get; set; }
    public DateTime LastMessageTime { get; set; }
    public bool IsOnline { get; set; }
    public bool IsGroup { get; set; }
    public List<UserModel> Participants { get; set; }

}