using System;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;

namespace CoreHook.Memory
{
    public interface IProcessManager2 : IProcess
    {
        void OpenHandle(Process process);
        void InjectBinary(string modulePath);
        IntPtr Execute(string module, string function, byte[] arguments, bool canWait = true);
        bool FreeMemory(IntPtr address, int? size = null);
        IntPtr CopyToProcess(byte[] data, int? size = null);
    }
    public interface IProcessManager : IProcess, IDisposable
    {
        void OpenHandle(Process process);
        void InjectBinary(string modulePath);
        IntPtr Execute(string module, string function, byte[] arguments, bool canWait = true);
        IntPtr CopyToProcess(byte[] data, int? size = null);
    }
}
