using System;
using System.Diagnostics;

namespace CoreHook.Memory
{
    public interface IProcessManager : IProcess, IDisposable
    {
        void OpenHandle(Process process);
        void InjectBinary(string modulePath);
        IntPtr Execute(string module, string function, byte[] arguments, bool canWait = true);
        IntPtr CopyToProcess(byte[] data, int? size = null);
    }
}
