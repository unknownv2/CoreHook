
namespace CoreHook.IPC.Messages
{
    public class InjectionCompleteNotification
    {
        public const string InjectionComplete = "InjectionComplete";

        public InjectionCompleteMessage RequestData { get; }

        public InjectionCompleteNotification(string body)
        {
            RequestData = InjectionCompleteMessage.FromBody(body);
        }

        public InjectionCompleteNotification(int processId, bool didComplete)
        {
            RequestData = new InjectionCompleteMessage(didComplete, processId);
        }

        public IMessage CreateMessage()
        {
            return new Message(InjectionComplete, RequestData.ToMessage());
        }

        public static IMessage CreateMessage(int processId, bool didComplete)
        {
            return new InjectionCompleteNotification(processId, didComplete).CreateMessage();
        }

        public static InjectionCompleteMessage ParseMessage(IMessage message)
        {
            return InjectionCompleteMessage.FromBody(message.Body);
        }
    }
}
