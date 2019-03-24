
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

        public IStringMessage CreateMessage()
        {
            return new StringMessage(InjectionComplete, RequestData.ToMessage());
        }

        public static IStringMessage CreateMessage(int processId, bool didComplete)
        {
            return new InjectionCompleteNotification(processId, didComplete).CreateMessage();
        }

        public static InjectionCompleteMessage ParseMessage(IStringMessage message)
        {
            return InjectionCompleteMessage.FromBody(message.Body);
        }
    }
}
