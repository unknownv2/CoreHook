using CoreHook.IPC.Messages;

namespace CoreHook.IPC
{
    public interface IMessageWriter
    {
        bool TryWrite(IMessage message);
        void Write(IMessage message);
    }
}
