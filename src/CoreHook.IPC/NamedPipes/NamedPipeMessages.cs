using System;
using Newtonsoft.Json;

namespace CoreHook.IPC.NamedPipes
{
    public static partial class NamedPipeMessages
    {
        private const string ResponseSuffix = "Response";
        public const string UnknownRequest = "UnknownRequest";
        private const char MessageSeparator = '|';

        public interface IMessage
        {
            string Header { get; }

            string Body { get; }

            string ToMessage();
        }
        public interface ICustomMessage
        {
            string ToMessage();
        }
        public class Message : IMessage
        {
            public string Header { get; }
            public string Body { get; }

            public Message(string header, string body)
            {
                Header = header;
                Body = body;
            }

            public static IMessage FromString(string message)
            {
                string header = null;
                string body = null;
                if (!string.IsNullOrEmpty(message))
                {
                    string[] parts = message.Split(new[] { NamedPipeMessages.MessageSeparator }, count: 2);
                    header = parts[0];
                    if (parts.Length > 1)
                    {
                        body = parts[1];
                    }
                }
                return new Message(header, body);
            }

            public override string ToString()
            {
                string result = string.Empty;
                if (!string.IsNullOrEmpty(Header))
                {
                    result = Header;
                }
                if (Body != null)
                {
                    result = result + NamedPipeMessages.MessageSeparator + Body;
                }
                return result;
            }
            public virtual string ToMessage()
            {
                return ToString();
            }
        }

        public class InjectionCompleteNotification
        {
            public const string InjectionComplete = "InjectionComplete";

            public InjectionCompleteNotification(string body)
            {
                RequestData = InjectionCompleteMessage.FromBody(body);
            }
            public InjectionCompleteNotification(int pid, bool didComplete)
            {
                RequestData = new InjectionCompleteMessage(pid, didComplete);
            }

            public InjectionCompleteMessage RequestData { get; }

            public Message CreateMessage()
            {
                return new Message(InjectionComplete, RequestData.ToMessage());
            }
        }

        public class InjectionCompleteMessage : ICustomMessage
        {
            public InjectionCompleteMessage(int pid, bool didComplete)
            {
                PID = pid;
                Completed = didComplete;
            }

            public int PID { get; set; }

            public bool Completed{ get; set; }

            internal static InjectionCompleteMessage FromBody(string body)
            {
                if (!string.IsNullOrEmpty(body))
                {
                    string[] dataParts = body.Split(MessageSeparator);
                    int pid;
                    bool didComplete = false;

                    if (dataParts.Length < 2)
                    {
                        throw new InvalidOperationException($"Invalid complete message. Expected at least 2 parts, got: {dataParts.Length} from message: '{body}'");
                    }

                    if (!int.TryParse(dataParts[0], out pid))
                    {
                        throw new InvalidOperationException($"Invalid complete message. Expected PID, got: {dataParts[0]} from message: '{body}'");
                    }

                    if (!bool.TryParse(dataParts[1], out didComplete))
                    {
                        throw new InvalidOperationException($"Invalid complete message. Expected bool for didComplete, got: {dataParts[1]} from message: '{body}'");
                    }

                    return new InjectionCompleteMessage(pid, didComplete);
                }

                return null;
            }

            public string ToMessage()
            {
                return string.Join(MessageSeparator.ToString(), PID, Completed);
            }
        }
    }
}
