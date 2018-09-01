using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoreHook.BinaryInjection
{
    public interface IBinaryLoader : IDisposable
    {
        void Load(Process targetProcess, string binaryPath, IEnumerable<string> dependencies = null, string dir = null);
        void CallFunctionWithRemoteArgs(Process process, string module, string function, BinaryLoaderArgs blArgs, RemoteFunctionArgs rfArgs);
        IntPtr CopyMemoryTo(Process proc, byte[] buffer, uint length);
    }
}