using CoreHook.IPC.Messages;

namespace CoreHook.IPC
{
    /// <summary>
    /// Interface for a message handler that can receive messages.
    /// </summary>
    public interface IMessageReader
    {
        /// <summary>
        /// Read an incoming message.
        /// </summary>
        /// <returns>A message from a remote process.</returns>
        IStringMessage Read();
    }
}
