using System;

namespace CoreHook.Memory.Processes
{
    public interface IProcessManager : IDisposable
    {
        void InjectBinary(string modulePath);
        IntPtr Execute(string module, string function, byte[] arguments, bool waitForThreadExit = true);
        IntPtr CopyToProcess(byte[] data, int? size = null);
    }
}
