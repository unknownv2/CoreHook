using CoreHook.IPC.Handlers;

namespace CoreHook.IPC.Transport
{
    public interface ITransportChannel
    {
        IConnection Connection { get; }
        IMessageHandler MessageHandler { get; }
    }
}
