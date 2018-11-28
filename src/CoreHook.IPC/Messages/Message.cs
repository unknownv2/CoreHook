
namespace CoreHook.IPC.Messages
{
    public class Message : CustomMessage, IMessage
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
                string[] parts = message.Split(new[] { MessageSeparator }, 2);
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
                result = result + MessageSeparator + Body;
            }
            return result;
        }
        public override string ToMessage()
        {
            return ToString();
        }
    }
}
