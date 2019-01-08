using System;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;

namespace CoreHook.Memory.Processes
{
    public sealed partial class ManagedProcess : IProcess
    {
        public Process ProcessHandle { get; }
        public SafeProcessHandle SafeHandle { get; }

        private const int DefaultProcessAccess =
                Interop.Advapi32.ProcessOptions.PROCESS_CREATE_THREAD |
                Interop.Advapi32.ProcessOptions.PROCESS_QUERY_INFORMATION |
                Interop.Advapi32.ProcessOptions.PROCESS_VM_OPERATION |
                Interop.Advapi32.ProcessOptions.PROCESS_VM_READ |
                Interop.Advapi32.ProcessOptions.PROCESS_VM_WRITE;

        public ManagedProcess(Process process, int access = DefaultProcessAccess)
        {
            ProcessHandle = process;
            SafeHandle = GetProcessHandle(process.Id, access);
        }

        private SafeProcessHandle GetProcessHandle(int processId, int access)
        {
            return Interop.Kernel32.OpenProcess(access, false, processId)
                ?? throw new UnauthorizedAccessException($"Failed to open process handle with access {access}.");
        }
    }
}
