using System.IO;
using CoreHook.IPC.Messages;
using CoreHook.IPC.Transport;

namespace CoreHook.IPC
{
    public class MessageReader : IMessageReader
    {
        private readonly StreamReader _reader;

        public MessageReader(IConnection connection)
        {
            _reader = new StreamReader(connection.Stream);
        }

        public IMessage Read()
        {
            try
            {
                return Message.FromString(_reader.ReadLine());
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}
