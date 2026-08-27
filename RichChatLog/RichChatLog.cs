using CkCommons.Classes;

namespace CkCommons.RichChat;

public abstract class RichChatLog<T> where T : IChatMessage
{
    /// <summary> Can be changed, acts as the chatlog identifier. </summary>
    protected string chatlogId;

    /// <summary> The stored messages for this chatlog. </summary>
    protected CircularBuffer<T> messages;

    /// <summary> The MsgIds where we were mentioned. </summary>
    protected HashSet<string> mentionIds;

    protected int unreadTotal = 0;

    public RichChatLog(string id, int capacity)
    {
        chatlogId = id;
        messages = new CircularBuffer<T>(capacity);
        mentionIds = new(StringComparer.Ordinal);
    }

    // To remember between chatlogs.
    public string PreviewMessage = string.Empty;
    public bool AutoScroll = true;

    public string ID => chatlogId;
    public IReadOnlyCollection<T> Messages => messages;

    public int UnreadMessages => unreadTotal;
    public int UnreadMentions => mentionIds.Count;

    public bool NewSenderSinceLastMsg(string senderId)
        => messages.IsEmpty || messages.Back().SenderId != senderId;

    public void MarkAsRead(bool clearMentions = false)
    {
        unreadTotal = 0;
        if (clearMentions)
            mentionIds.Clear();
    }

    protected void AddLogMessage(T message)
    {
        messages.PushBack(message);
        unreadTotal++;
        if (message.WasMentioned)
            mentionIds.Add(message.MsgId);
    }

    protected void ClearLog()
    {
        messages.Clear();
        mentionIds.Clear();
        unreadTotal = 0;
    }
}