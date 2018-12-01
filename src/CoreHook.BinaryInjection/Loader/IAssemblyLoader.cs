using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    interface IAssemblyLoader : IDisposable
    {
        void LoadModule(string path);
        void CreateThread(IRemoteFunctionCall call, bool waitForThreadExit = true);
        IntPtr CopyMemory(byte[] buffer, int length);
    }
}
