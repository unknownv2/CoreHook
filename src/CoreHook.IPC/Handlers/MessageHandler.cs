using CoreHook.IPC.Messages;

namespace CoreHook.IPC.Handlers
{
    /// <summary>
    /// Sends and receive messages using a connection provider.
    /// </summary>
    public class MessageHandler : IMessageHandler
    {
        private readonly IMessageReader _messageReader;
        private readonly IMessageWriter _messageWriter;

        /// <summary>
        /// Initialize a new instance of the <see cref="MessageHandler"/> class.
        /// </summary>
        /// <param name="connection">The communication provider which messages are sent to and
        /// received from.</param>
        public MessageHandler(IConnection connection)
        {
            _messageReader = new MessageReader(connection);
            _messageWriter = new MessageWriter(connection);
        }

        /// <inheritdoc />
        public IMessage Read() => _messageReader.Read();

        /// <inheritdoc />
        public bool TryWrite(IMessage message) => _messageWriter.TryWrite(message);

        /// <inheritdoc />
        public void Write(IMessage message) => _messageWriter.Write(message);
    }
}
