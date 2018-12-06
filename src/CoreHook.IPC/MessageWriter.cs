using System.IO;
using CoreHook.IPC.Messages;
using CoreHook.IPC.Transport;

namespace CoreHook.IPC
{
    public class MessageWriter : IMessageWriter
    {
        private readonly StreamWriter _writer;

        public MessageWriter(IConnection connection)
        {
            _writer = new StreamWriter(connection.Stream);
        }

        public bool TryWrite(IMessage message)
        {
            try
            {
                Write(message);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public void Write(IMessage message)
        {
            Write(message.ToString());
        }

        public void Write(string message)
        {
            _writer.WriteLine(message);
            _writer.Flush();
        }
    }
}
