using System;

namespace CoreHook.Memory.Processes
{
    public interface IThreadManager
    {
        IntPtr CreateThread(string module, string function, byte[] arguments, bool waitForThreadExit = true);
    }
}
