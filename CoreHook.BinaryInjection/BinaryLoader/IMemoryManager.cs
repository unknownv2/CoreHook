using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CoreHook.BinaryInjection
{
    public delegate bool FreeMemory(Process proc, IntPtr address, uint length = 0);

    public interface IMemoryManager : IDisposable
    {
        IntPtr Add(Process process, IntPtr address, bool isFree, uint size = 0);
        void FreeAll();
    }
}
