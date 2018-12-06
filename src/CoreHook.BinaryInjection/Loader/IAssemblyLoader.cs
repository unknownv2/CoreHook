using System;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IAssemblyLoader : IDisposable
    {
        void LoadModule(string path);
        IntPtr CopyMemory(byte[] buffer, int length);
        void CreateThread(IRemoteFunctionCall call, bool waitForThreadExit = true);
    }
}
