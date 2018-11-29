using System;
using System.Collections.Generic;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IBinaryLoader : IDisposable
    {
        void Load(string binaryPath, IEnumerable<string> dependencies = null, string baseDirectory = null);

        void ExecuteRemoteFunction(IRemoteFunctionCall call);

        void ExecuteRemoteManagedFunction(IRemoteManagedFunctionCall call);

        IntPtr CopyMemoryTo(byte[] buffer, int length);
    }
}