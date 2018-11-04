using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

namespace CoreHook.IPC.NamedPipes
{
    public class NamedPipeClient : INamedPipeClient
    {
        private readonly string _pipeName;
       
        private NamedPipeClientStream _clientStream;

        private StreamReader _reader;
        private StreamWriter _writer;

        public PipeStream PipeStream => _clientStream;

        private const string ServerName = ".";

        public NamedPipeClient(string pipeName)
        {
            _pipeName = pipeName;
        }

        public static PipeStream CreatePipeStream(string pipeName)
        {
            var client = new NamedPipeClient(pipeName);
            return client.Connect() ? client.PipeStream : null;
        }

        public bool Connect(int timeoutMilliseconds = 3000)
        {
            if (_clientStream != null)
            {
                throw new InvalidPipeOperationException("Client pipe already connected");
            }
            if (_pipeName == null)
            {
                throw new InvalidPipeOperationException("Client pipe name was not set");
            }
            try
            {
                _clientStream = new NamedPipeClientStream(
                    ServerName,
                    _pipeName,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous,
                    TokenImpersonationLevel.Impersonation);
                    _clientStream.Connect(timeoutMilliseconds);
            }
            catch (TimeoutException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            _reader = new StreamReader(_clientStream);
            _writer = new StreamWriter(_clientStream);
            return true;
        }

        public bool TrySendRequest(NamedPipeMessages.Message message)
        {
            try
            {
                SendRequest(message);
                return true;
            }
            catch (InvalidPipeOperationException)
            {
            }
            return false;
        }

        public void SendRequest(NamedPipeMessages.Message message)
        {
            SendRequest(message.ToString());
        }

        public void SendRequest(string message)
        {
            ValidateConnection();

            try
            {
                _writer.WriteLine(message);
                _writer.Flush();
            }
            catch (IOException e)
            {
                throw new InvalidPipeOperationException("Unable to send: " + message, e);
            }
        }

        public string ReadRawResponse()
        {
            try
            {
                string response = _reader.ReadLine();
                if (response == null)
                {
                    throw new InvalidPipeOperationException("Unable to read from pipe", null);
                }
                return response;
            }
            catch (IOException e)
            {
                throw new InvalidPipeOperationException("Unable to read from pipe", e);
            }
        }

        public NamedPipeMessages.IMessage ReadResponse()
        {
            return NamedPipeMessages.Message.FromString(ReadRawResponse());
        }

        public bool TryReadResponse(out NamedPipeMessages.IMessage message)
        {
            try
            {
                message = NamedPipeMessages.Message.FromString(ReadRawResponse());
                return true;
            }
            catch (InvalidPipeOperationException)
            {
                message = null;
                return false;
            }
        }

        public void Dispose()
        {
            ValidateConnection();

            _clientStream?.Dispose();
            _clientStream = null;

            _reader = null;
            _writer = null;
        }

        private void ValidateConnection()
        {
            if (_clientStream == null)
            {
                throw new InvalidPipeOperationException("There is no connection");
            }
        }
    }
}
