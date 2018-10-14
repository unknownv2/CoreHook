using System;
using System.Diagnostics;

namespace CoreHook.Unmanaged
{
    public interface IProcessManager
    {
        void InjectBinary(string modulePath);
        IntPtr Execute(string module, string function, byte[] args, bool canWait = true);
        void OpenHandle(Process process);
        bool FreeMemory(IntPtr address, int? size = null);
        IntPtr MemCopyTo(byte[] data, int? size = null);
    }
}
