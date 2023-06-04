using CoreHook.IPC.Messages;

using System.IO;
using System.IO.Pipes;

namespace CoreHook.IPC.Handlers;

/// <summary>
/// Sends and receive messages using a connection provider.
/// </summary>
public class MessageHandler : IMessageHandler
{
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    /// <summary>
    /// Initialize a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="connection">The communication provider which messages are sent to and
    /// received from.</param>
    public MessageHandler(PipeStream stream)
    {
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream);
    }


    /// <inheritdoc />
    public IStringMessage Read()
    {
        try
        {
            return StringMessage.FromString(_reader.ReadLine());
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public bool TryWrite(IStringMessage message)
    {
        try
        {
            _writer.WriteLine(message.ToString());
            _writer.Flush();
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
