
namespace CoreHook.IPC.Messages
{
    /// <summary>
    /// Interface for defining a message that can be sent and received between processes.
    /// </summary>
    public interface IMessage
    {
        string Header { get; }
        string Body { get; }
    }
}
