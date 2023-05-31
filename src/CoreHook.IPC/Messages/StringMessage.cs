
namespace CoreHook.IPC.Messages;

/// <summary>
/// A representation of data containing information to be communicated between a client and server.
/// </summary>
public class StringMessage : CustomMessage, IStringMessage
{
    /// <inheritdoc />
    public string Header { get; }
    /// <inheritdoc />
    public string Body { get; }

    /// <summary>
    /// Initialize a new instance of the <see cref="StringMessage"/> class.
    /// </summary>
    /// <param name="header">The message properties.</param>
    /// <param name="body">The message data.</param>
    public StringMessage(string header, string body)
    {
        Header = header;
        Body = body;
    }

    /// <summary>
    /// Parse a string message.
    /// </summary>
    /// <param name="message">The message data.</param>
    /// <returns>A new instance of the message class.</returns>
    public static IStringMessage FromString(string message)
    {
        string header = null;
        string body = null;
        if (!string.IsNullOrEmpty(message))
        {
            string[] parts = message.Split(new[] { MessageSeparator }, 2);
            header = parts[0];
            if (parts.Length > 1)
            {
                body = parts[1];
            }
        }
        return new StringMessage(header, body);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        string result = string.Empty;
        if (!string.IsNullOrEmpty(Header))
        {
            result = Header;
        }
        if (Body is not null)
        {
            result = result + MessageSeparator + Body;
        }
        return result;
    }

    /// <inheritdoc />
    public override string ToMessage()
    {
        return ToString();
    }
}
