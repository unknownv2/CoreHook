using System;
using System.IO;
using System.IO.Pipes;

namespace CoreHook.IPC
{
    internal class PipeConnection : IConnection
    {
        public Stream Stream => _stream;

        private readonly Func<bool> _connectionEnded;
        private readonly PipeStream _stream;

        public PipeConnection(PipeStream stream, Func<bool> connectionEnded)
        {
            _connectionEnded = connectionEnded;
            _stream = stream;
        }

        public bool IsConnected => !_connectionEnded() && _stream.IsConnected;
    }
}
