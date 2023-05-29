using System.IO;
using CoreHook.IPC.Messages;

namespace CoreHook.IPC;

/// <summary>
/// Sends messages over a user-defined communication protocol.
/// </summary>
public class MessageWriter : IMessageWriter
{
    private readonly StreamWriter _writer;

    /// <summary>
    /// Initialize a new instance of the <see cref="MessageWriter"/> class.
    /// </summary>
    /// <param name="connection">The communication provider which messages are over.</param>
    public MessageWriter(IConnection connection)
    {
        _writer = new StreamWriter(connection.Stream);
    }

    /// <inheritdoc />
    public bool TryWrite(IStringMessage message)
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

    /// <inheritdoc />
    public void Write(IStringMessage message)
    {
        Write(message.ToString());
    }

    /// <summary>
    /// Send a message formatted as a string.
    /// </summary>
    /// <param name="message">The information to send.</param>
    private void Write(string message)
    {
        _writer.WriteLine(message);
        _writer.Flush();
    }
}
