using System;

namespace CoreHook.BinaryInjection.Loader
{
    interface IAssemblyLoader : IDisposable
    {
        void LoadModule(string path);
        void CreateThread(IRemoteFunctionCall call, bool waitForThreadExit = true);
        IntPtr CopyMemory(byte[] buffer, int length);
    }
}
