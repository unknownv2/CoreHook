using CoreHook.IPC.Handlers;

namespace CoreHook.IPC.Transport
{
    /// <summary>
    /// Interface for sending and receiving messages over a stream.
    /// </summary>
    public interface ITransportChannel
    {
        /// <summary>
        /// Provides access to communication between a sender and receiver.
        /// </summary>
        IConnection Connection { get; }
        /// <summary>
        /// Manages sending and receiving messages over the <see cref="Connection"/>.
        /// </summary>
        IMessageHandler MessageHandler { get; }
    }
}
