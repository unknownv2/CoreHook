
namespace CoreHook.IPC.Messages;

/// <summary>
/// Define the basic template for creating message data.
/// </summary>
public abstract class CustomMessage
{
    /// <summary>
    /// A unique character that separates a list of messages.
    /// </summary>
    protected const char MessageSeparator = '|';

    /// <summary>
    /// Serialize the message's properties and data to a string.
    /// </summary>
    /// <returns>The message's properties and data in a string format.</returns>
    public abstract string ToMessage();
}
