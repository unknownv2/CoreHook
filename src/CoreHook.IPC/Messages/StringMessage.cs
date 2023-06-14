
using System.Text.Json.Serialization;

namespace CoreHook.IPC.Messages;

/// <summary>
/// A representation of data containing information to be communicated between a client and server.
/// </summary>
public class StringMessage : CustomMessage
{
    private string _body;

    /// <summary>
    /// Initialize a new instance of the <see cref="StringMessage"/> class.
    /// </summary>
    /// <param name="body">The message data.</param>
    public StringMessage(string body)
    {
        _body = body;
    }

    public override string ToString()
    {
        return _body;
    }
}
