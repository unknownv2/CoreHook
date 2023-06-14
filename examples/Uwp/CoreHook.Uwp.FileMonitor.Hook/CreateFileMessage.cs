using CoreHook.IPC.Messages;

namespace CoreHook.Uwp.FileMonitor.Hook;
public class CreateFileMessage : CustomMessage
{
    public string[] Queue { get; set; } 
}
