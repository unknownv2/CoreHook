
using CoreHook.IPC.Messages;

namespace CoreHook.IPC.Handlers;

/// <summary>
/// Interface for receiving and sending messages.
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// Attempt to send a message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>True if the message was sent successfully.</returns>
    bool TryWrite(IStringMessage message);
   
    /// <summary>
    /// Read an incoming message.
    /// </summary>
    /// <returns>A message from a remote process.</returns>
    IStringMessage Read();
}
