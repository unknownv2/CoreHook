// From https://github.com/Microsoft/VFSForGit/tree/master/GVFS/GVFS.Common/NamedPipes
using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

namespace CoreHook.IPC.NamedPipes
{
    public class NamedPipeClient : IDisposable
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
                clientStream = new NamedPipeClientStream(
                    serverName,
                    pipeName,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous,
                    TokenImpersonationLevel.Impersonation);
                    clientStream.Connect(timeoutMilliseconds);
            }
            catch (TimeoutException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            reader = new StreamReader(clientStream);
            writer = new StreamWriter(clientStream);
            return true;
        }
        public bool TrySendRequest(NamedPipeMessages.Message message)
        {
            try
            {
                this.SendRequest(message);
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
                this.writer.WriteLine(message);
                this.writer.Flush();
            }
            catch (IOException e)
            {
                throw new BrokenPipeException("Unable to send: " + message, e);
            }
        }
        public void SendRequest(byte[] message)
        {
            ValidateConnection();
            try
            {
                this.writer.Write(message);
                this.writer.Flush();
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
        public NamedPipeMessages.Message ReadResponse()
        {
            return NamedPipeMessages.Message.FromString(ReadRawResponse());
        }
        public bool TryReadResponse(out NamedPipeMessages.Message message)
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
            this.ValidateConnection();
            if (this.clientStream != null)
            {
                this.clientStream.Dispose();
                this.clientStream = null;
            }
            this.reader = null;
            this.writer = null;
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
