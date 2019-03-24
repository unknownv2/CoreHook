
namespace CoreHook.IPC.Messages
{
    /// <summary>
    /// A wrapper for a message containing application status information.
    /// </summary>
    public class LogMessageNotification
    {
        /// <summary>
        /// The message type.
        /// </summary>
        public const string Message = "LogMessage";
        /// <summary>
        /// The message information.
        /// </summary>
        public LogMessage RequestData { get; }

        /// <summary>
        /// Initialize a new instance of the <see cref="LogMessageNotification"/> class.
        /// </summary>
        /// <param name="body">The message data to parse.</param>
        public LogMessageNotification(string body)
        {
            RequestData = LogMessage.FromBody(body);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="LogMessageNotification"/> class.
        /// </summary>
        /// <param name="level">The message type.</param>
        /// <param name="message">The message information.</param>
        public LogMessageNotification(LogLevel level, string message)
        {
            RequestData = new LogMessage(level, message);
        }

        /// <summary>
        /// Serialize the current message data to message class.
        /// </summary>
        /// <returns></returns>
        public IStringMessage CreateMessage()
        {
            return new StringMessage(Message, RequestData.ToMessage());
        }

        /// <summary>
        /// Create a new message wrapper.
        /// </summary>
        /// <param name="level">The log message type.</param>
        /// <param name="message">The message data.</param>
        /// <returns>A new instance of a class representing a log message.</returns>
        public static IStringMessage CreateMessage(LogLevel level, string message)
        {
            return new LogMessageNotification(level, message).CreateMessage();
        }

        /// <summary>
        /// Parse a message's data.
        /// </summary>
        /// <param name="message">The message data.</param>
        /// <returns>A new instance of a class representing a log message.</returns>
        public static LogMessage ParseMessage(IStringMessage message)
        {
            return LogMessage.FromBody(message.Body);
        }
    }
}
