using System;

namespace CoreHook.Memory.Processes
{
    public interface IProcessManager : IModuleManager, IThreadManager, IDisposable
    {
        IntPtr CopyToProcess(byte[] data, int? size = null);
    }
}
