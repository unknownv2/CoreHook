using CoreHook.IPC.Messages;

namespace CoreHook.IPC
{
    public interface IMessageReader
    {
        IMessage Read();
    }
}
