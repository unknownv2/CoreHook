
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
            RequestData = new InjectionCompleteMessage(processId, didComplete);
        }

        public Message CreateMessage()
        {
            return new Message(InjectionComplete, RequestData.ToMessage());
        }
    }
}
