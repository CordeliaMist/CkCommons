namespace CkCommons.Chat;

public record CkChatMessage(string Name, string Message, DateTime Timestamp)
{
    public virtual string UID => "UNK";
}
