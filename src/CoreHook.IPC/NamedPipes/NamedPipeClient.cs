using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using CoreHook.IPC.Handlers;

namespace CoreHook.IPC.NamedPipes
{
    public class NamedPipeClient : INamedPipe
    {
        public IConnection Connection { get; private set; }
        public IMessageHandler MessageHandler { get; private set; }

        private NamedPipeClientStream _pipeStream;

        private bool _connectionStopped;

        private readonly string _pipeName;

        public NamedPipeClient(string pipeName)
        {
            _pipeName = pipeName;
            _connectionStopped = false;
        }

        public static PipeStream CreatePipeStream(string pipeName)
        {
            var client = new NamedPipeClient(pipeName);
            return client.Connect() ? client._pipeStream : null;
        }

        public bool Connect()
        {
            if (_pipeStream != null)
            {
                throw new InvalidPipeOperationException("Client pipe already connected");
            }
            if (_pipeName == null)
            {
                throw new InvalidPipeOperationException("Client pipe name was not set");
            }

            try
            {
                _pipeStream = new NamedPipeClientStream(
                    ".",
                    _pipeName,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous,
                    TokenImpersonationLevel.Impersonation);

                _pipeStream.Connect();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            Connection = new PipeConnection(_pipeStream, () => _connectionStopped);
            MessageHandler = new MessageHandler(Connection);

            return true;
        }

        public void Dispose()
        {
            _connectionStopped = true;
            _pipeStream?.Dispose();
            _pipeStream = null;
        }
    }
}
