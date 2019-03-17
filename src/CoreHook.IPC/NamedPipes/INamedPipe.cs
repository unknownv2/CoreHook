using System;
using CoreHook.IPC.Transport;

namespace CoreHook.IPC.NamedPipes
{
    /// <summary>
    /// Interface defining a named pipe communication channel.
    /// </summary>
    public interface INamedPipe : ITransportChannel, IDisposable
    {
        /// <summary>
        /// Initialize the pipe connection.
        /// </summary>
        /// <returns>True if initialization completed successfully.</returns>
        bool Connect();
    }
}
