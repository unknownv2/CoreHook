using System.Text.Json.Serialization;

namespace CoreHook.IPC.Messages;

/// <summary>
/// A message containing application status information.
/// </summary>
public class LogMessage : CustomMessage
{
    /// <summary>
    /// The message type.
    /// </summary>
    public LogLevel Level { get; }

    /// <summary>
    /// The message data.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initialize a new instance of the <see cref="LogMessage"/> class.
    /// </summary>
    /// <param name="level">The message type.</param>
    /// <param name="message">The message data.</param>
    public LogMessage(LogLevel level, string message)
    {
        Level = level;
        Message = message;
    }
}
