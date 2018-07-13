using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CoreHook.BinaryInjection
{
    public interface IMemoryManager : IDisposable
    {
        Func<Process, IntPtr, uint, bool> FreeMemory { get; set; }

        IntPtr Add(Process process, IntPtr address, bool isFree, uint size = 0);

        void FreeAllocations();
    }
}
