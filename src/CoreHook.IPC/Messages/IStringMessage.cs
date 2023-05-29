
namespace CoreHook.IPC.Messages;

/// <summary>
/// Defines a message that can be sent and received between processes.
/// </summary>
public interface IStringMessage
{
    /// <summary>
    /// The message's properties, such as message type.
    /// </summary>
    string Header { get; }
    /// <summary>
    /// The message information.
    /// </summary>
    string Body { get; }
}
