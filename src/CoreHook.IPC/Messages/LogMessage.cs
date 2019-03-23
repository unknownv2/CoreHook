using System;
namespace CoreHook.IPC.Messages
{
    /// <summary>
    /// Defines the types of message information.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug diagnostic message.
        /// </summary>
        Debug = 0,
        /// <summary>
        /// Release diagnostic message.
        /// </summary>
        Trace,
        /// <summary>
        /// General information message.
        /// </summary>
        Info,
        /// <summary>
        /// Warning message.
        /// </summary>
        Warning,
        /// <summary>
        /// Error message.
        /// </summary>
        Error
    }

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

        /// <summary>
        /// Parse a log message from it's string data.
        /// </summary>
        /// <param name="body">The message data.</param>
        /// <returns>A new instance of <see cref="LogMessage"/>.</returns>
        internal static LogMessage FromBody(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return null;
            }

            string[] dataParts = body.Split(MessageSeparator);

            if (dataParts.Length < 2)
            {
                throw new InvalidOperationException($"Invalid complete message. Expected at least 2 parts, got: {dataParts.Length} from message: '{body}'");
            }

            if (!Enum.TryParse<LogLevel>(dataParts[0], out var logLevel))
            {
                throw new InvalidOperationException($"Invalid complete message. Expected LogLevel, got: {dataParts[0]} from message: '{body}'");
            }

            return new LogMessage(logLevel, dataParts[1]);
        }

        /// <inheritdoc />
        public override string ToMessage()
        {
            return string.Join(MessageSeparator.ToString(), Level, Message);
        }
    }
}
