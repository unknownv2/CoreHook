using System.IO;

namespace CoreHook.IPC
{
    /// <summary>
    /// Interface defining a stream-based connection, for example with inter-process pipe streams.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// I/O for sending and receiving messages.
        /// </summary>
        Stream Stream { get; }
        /// <summary>
        /// True if the connection is active.
        /// </summary>
        bool IsConnected { get; }
    }
}
