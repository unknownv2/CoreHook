using System;
using System.Diagnostics;

namespace CoreHook.Memory
{
    public interface IProcessManager
    {
        void InjectBinary(string modulePath);
        IntPtr Execute(string module, string function, byte[] arguments, bool canWait = true);
        void OpenHandle(Process process);
        bool FreeMemory(IntPtr address, int? size = null);
        IntPtr MemCopyTo(byte[] data, int? size = null);
    }
}
