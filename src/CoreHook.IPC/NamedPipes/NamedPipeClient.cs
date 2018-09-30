using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

namespace CoreHook.IPC.NamedPipes
{
    public class NamedPipeClient : INamedPipeClient
    {
        private string pipeName;
        private NamedPipeClientStream clientStream;
        private StreamReader reader;
        private StreamWriter writer;
        private const string serverName = ".";

        public NamedPipeClient(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public bool Connect(int timeoutMilliseconds = 3000)
        {
            if (clientStream != null)
            {
                throw new InvalidOperationException();
            }
            if (pipeName == null)
            {
                throw new InvalidOperationException("Client pipe name was not set");
            }
            try
            {
                this.clientStream = new NamedPipeClientStream(
                    serverName,
                    this.pipeName,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous,
                    TokenImpersonationLevel.Impersonation);
                    this.clientStream.Connect(timeoutMilliseconds);
            }
            catch (TimeoutException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            this.reader = new StreamReader(this.clientStream);
            this.writer = new StreamWriter(this.clientStream);
            return true;
        }

        public bool TrySendRequest(NamedPipeMessages.Message message)
        {
            try
            {
                SendRequest(message);
                return true;
            }
            catch (BrokenPipeException)
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
                writer.WriteLine(message);
                writer.Flush();
            }
            catch (IOException e)
            {
                throw new BrokenPipeException("Unable to send: " + message, e);
            }
        }

        public string ReadRawResponse()
        {
            try
            {
                string response = reader.ReadLine();
                if (response == null)
                {
                    throw new BrokenPipeException("Unable to read from pipe", null);
                }
                return response;
            }
            catch (IOException e)
            {
                throw new BrokenPipeException("Unable to read from pipe", e);
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
            catch (BrokenPipeException)
            {
                message = null;
                return false;
            }
        }

        public void Dispose()
        {
            ValidateConnection();

            clientStream?.Dispose();
            clientStream = null;

            reader = null;
            writer = null;
        }

        private void ValidateConnection()
        {
            if (clientStream == null)
            {
                throw new InvalidOperationException("There is no connection");
            }
        }
    }
}
