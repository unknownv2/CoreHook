using CoreHook.IPC.Messages;

namespace CoreHook.IPC.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IMessageReader _messageReader;
        private readonly IMessageWriter _messageWriter;

        public MessageHandler(IConnection connection)
        {
            _messageReader = new MessageReader(connection);
            _messageWriter = new MessageWriter(connection);
        }

        public IMessage Read() => _messageReader.Read();

        public bool TryWrite(IMessage message) => _messageWriter.TryWrite(message);

        public void Write(IMessage message) => _messageWriter.Write(message);
    }
}
