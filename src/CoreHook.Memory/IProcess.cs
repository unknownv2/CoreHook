using Microsoft.Win32.SafeHandles;

namespace CoreHook.Memory
{
    public interface IProcess
    {
        System.Diagnostics.Process ProcessHandle { get; }

        int ProcessId { get; }

        SafeProcessHandle SafeHandle { get; }
    }
}
