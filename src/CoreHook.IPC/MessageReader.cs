using System.IO;
using CoreHook.IPC.Messages;

namespace CoreHook.IPC
{
    /// <summary>
    /// Reads messages from a user-defined communication protocol.
    /// </summary>
    public class MessageReader : IMessageReader
    {
        private readonly StreamReader _reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReader"/> class.
        /// </summary>
        /// <param name="connection">The communication provider which messages are from.</param>
        public MessageReader(IConnection connection)
        {
            _reader = new StreamReader(connection.Stream);
        }

        /// <inheritdoc />
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
