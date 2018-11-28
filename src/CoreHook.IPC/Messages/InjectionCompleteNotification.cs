
namespace CoreHook.IPC.Messages
{
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
}
