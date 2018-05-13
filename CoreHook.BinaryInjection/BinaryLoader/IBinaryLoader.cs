using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace CoreHook.BinaryInjection
{
    public interface IBinaryLoader
    {
        void Load(Process targetProcess, string binaryPath, IEnumerable<string> dependencies, string dir);
    }
}
