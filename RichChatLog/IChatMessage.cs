namespace CkCommons.RichChat;

/// <summary>
///   Required Interface for ChatMessages used in a CkChatLog.
/// </summary>
public interface IChatMessage
{
    /// <summary>
    ///   The unique identifier of this message.
    /// </summary>
    string MsgId { get; }

    /// <summary>
    ///   The Identifier of the sender. Typically a user ID or username.
    /// </summary>
    string SenderId { get; }

    /// <summary>
    ///   When the message was sent, in UTC.
    /// </summary>
    DateTime TimestampUTC { get; }
    
    /// <summary>
    ///   The message content.
    /// </summary>
    string Message { get; set; }
    
    /// <summary>
    ///   For Mentions
    /// </summary>
    bool WasMentioned { get; set; }
}