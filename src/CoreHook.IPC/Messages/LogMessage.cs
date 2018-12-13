using System;
namespace CoreHook.IPC.Messages
{
    public enum LogLevel
    {
        Debug = 0,
        Trace,
        Info,
        Warning,
        Error
    }

    public class LogMessageNotification
    {
        public const string Message = "LogMessage";
        public LogMessage RequestData { get; }

        public LogMessageNotification(string body)
        {
            RequestData = LogMessage.FromBody(body);
        }

        public LogMessageNotification(LogLevel level, string message)
        {
            RequestData = new LogMessage(level, message);
        }

        public IMessage CreateMessage()
        {
            return new Message(Message, RequestData.ToMessage());
        }

        public static IMessage CreateMessage(LogLevel level, string message)
        {
            return new LogMessageNotification(level, message).CreateMessage();
        }

        public static LogMessage ParseMessage(IMessage message)
        {
            return LogMessage.FromBody(message.Body);
        }
    }

    public class LogMessage : CustomMessage
    {
        public LogLevel Level { get; }

        public string Message { get; }

        public LogMessage(LogLevel level, string message)
        {
            Level = level;
            Message = message;
        }

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

        public override string ToMessage()
        {
            return string.Join(MessageSeparator.ToString(), Level, Message);
        }
    }
}
