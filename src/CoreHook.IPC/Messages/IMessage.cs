
namespace CoreHook.IPC.Messages
{
    public interface IMessage
    {
        string Header { get; }

        string Body { get; }

        string ToMessage();
    }
}
