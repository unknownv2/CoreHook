using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace CoreHook.BinaryInjection
{
    public interface IBinaryLoader : IDisposable
    {
        void Load(Process targetProcess, string binaryPath, IEnumerable<string> dependencies = null, string dir = null);
        void Execute(Process process, string module, string function, string args);
        void ExecuteWithArgs(Process process, string module, object args);
        void CallFunctionWithRemoteArgs(Process process, string module, string function, IntPtr arguments);
        IntPtr CopyMemoryTo(Process proc, byte[] buffer, uint length);
    }
}