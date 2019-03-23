using System;
using System.IO;
using System.IO.Pipes;

namespace CoreHook.IPC
{
    /// <summary>
    /// A communication channel for pipes.
    /// </summary>
    internal class PipeConnection : IConnection
    {
        /// <inheritdoc />
        public Stream Stream => _stream;

        private readonly Func<bool> _connectionEnded;
        private readonly PipeStream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipeConnection"/> class.
        /// </summary>
        /// <param name="stream">Provides a method for communication.</param>
        /// <param name="connectionEnded">Determine if the communication is open or closed.</param>
        public PipeConnection(PipeStream stream, Func<bool> connectionEnded)
        {
            _connectionEnded = connectionEnded;
            _stream = stream;
        }

        /// <inheritdoc />
        public bool IsConnected => !_connectionEnded() && _stream.IsConnected;
    }
}
