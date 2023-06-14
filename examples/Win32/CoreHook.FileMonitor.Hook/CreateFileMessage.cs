using CoreHook.IPC.Messages;

namespace CoreHook.FileMonitor.Hook;
public class CreateFileMessage : CustomMessage
{
    public string[] Queue { get; set; } 
}
