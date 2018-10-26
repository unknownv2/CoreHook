using System;
using System.IO;
using System.IO.Pipes;
using CoreHook.IPC.NamedPipes;

namespace CoreHook.IPC
{
    public class Connection : IConnection
    {
        public NamedPipeServerStream ServerStream { get; }

        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly Func<bool> _isStopping;

        public Connection(NamedPipeServerStream serverStream, Func<bool> isStopping)
        {
            ServerStream = serverStream;

            _isStopping = isStopping;
            _reader = new StreamReader(ServerStream);
            _writer = new StreamWriter(ServerStream);
        }

        public bool IsConnected => !_isStopping() && ServerStream.IsConnected;

        public NamedPipeMessages.IMessage ReadMessage()
        {
            return NamedPipeMessages.Message.FromString(ReadRequest());
        }

        public string ReadRequest()
        {
            try
            {
                return _reader.ReadLine();
            }
            catch (IOException)
            {
                return null;
            }
        }

        public bool TrySendResponse(string message)
        {
            try
            {
                _writer.WriteLine(message);
                _writer.Flush();

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public bool TrySendResponse(NamedPipeMessages.Message message)
        {
            return TrySendResponse(message.ToString());
        }
    }
}
