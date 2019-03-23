using CoreHook.IPC.Messages;

namespace CoreHook.IPC
{
    /// <summary>
    /// Interface for a message handler that can send messages.
    /// </summary>
    public interface IMessageWriter
    {
        /// <summary>
        /// Attempt to send a message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the message was sent successfully.</returns>
        bool TryWrite(IMessage message);
        /// <summary>
        /// Send a message with no feedback on whether it was sent successfully.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void Write(IMessage message);
    }
}
