using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoreHook.IPC.NamedPipes
{
    public static partial class NamedPipeMessages
    {
        private const string ResponseSuffix = "Response";
        public const string UnknownRequest = "UnknownRequest";
        private const char MessageSeparator = '|';
        
        public enum CompletionState
        {
            NotCompleted,
            Success,
            Failure
        }

        public class BaseResponse<TRequest>
        {
            public const string Header = nameof(TRequest) + ResponseSuffix;
            public CompletionState State { get; set; }
            public string ErrorMessage { get; set; }
            public Message ToMessage()
            {
                return new Message(Header, JsonConvert.SerializeObject(this));
            }
        }

        public class Message
        {
            public Message(string header, string body)
            {
                Header = header;
                Body = body;
            }
            public string Header { get; }
            public string Body { get; }
            public static Message FromString(string message)
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
                if (!string.IsNullOrEmpty(this.Header))
                {
                    result = this.Header;
                }
                if (this.Body != null)
                {
                    result = result + NamedPipeMessages.MessageSeparator + this.Body;
                }
                return result;
            }
        }
        public class InjectionCompleteNotification
        {
            public const string InjectionComplete = "InjectionComplete";

            public InjectionCompleteNotification(string body)
            {
                this.RequestData = InjectionCompleteMessage.FromBody(body);
            }
            public InjectionCompleteNotification(int pid, bool didComplete)
            {
                this.RequestData = new InjectionCompleteMessage(pid, didComplete);
            }

            public InjectionCompleteMessage RequestData { get; }

            public Message CreateMessage()
            {
                return new Message(InjectionComplete, this.RequestData.ToMessage());
            }
        }
        public class InjectionCompleteMessage
        {
            public InjectionCompleteMessage(int pid, bool didComplete)
            {
                this.PID = pid;
                this.Completed = didComplete;
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
                        throw new InvalidOperationException(string.Format("Invalid complete message. Expected at least 3 parts, got: {0} from message: '{1}'", dataParts.Length, body));
                    }

                    if (!int.TryParse(dataParts[0], out pid))
                    {
                        throw new InvalidOperationException(string.Format("Invalid complete message. Expected PID, got: {0} from message: '{1}'", dataParts[0], body));
                    }

                    if (!bool.TryParse(dataParts[1], out didComplete))
                    {
                        throw new InvalidOperationException(string.Format("Invalid complete message. Expected bool for didComplete, got: {0} from message: '{1}'", dataParts[1], body));
                    }

                    return new InjectionCompleteMessage(pid, didComplete);
                }

                return null;
            }

            internal string ToMessage()
            {
                return string.Join(MessageSeparator.ToString(), this.PID, this.Completed);
            }
        }
    }
}
