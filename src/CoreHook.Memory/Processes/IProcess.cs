using Microsoft.Win32.SafeHandles;

namespace CoreHook.Memory.Processes
{
    public interface IProcess
    {
        System.Diagnostics.Process ProcessHandle { get; }
        SafeProcessHandle SafeHandle { get; }
    }
}
