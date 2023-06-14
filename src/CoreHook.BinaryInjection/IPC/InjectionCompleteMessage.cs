using CoreHook.IPC.Messages;

namespace CoreHook.BinaryInjection.IPC;

/// <summary>
/// A message containing information about an attempt to load
/// a plugin in a remote process. The message is sent from the
/// target application back to the host application that awaits it.
/// If the host application does not receive the message, then we assume
/// that the plugin loading has failed.
/// </summary>
public class InjectionCompleteMessage : CustomMessage
{
    /// <summary>
    /// True if the plugin load completed successfully, otherwise false.
    /// </summary>
    public bool Completed { get; }

    /// <summary>
    /// The process ID of the remote process in which the plugin has been loaded to.
    /// </summary>
    public int ProcessId { get; }

    public InjectionCompleteMessage(int processId, bool completed)
    {
        Completed = completed;
        ProcessId = processId;
    }
}
