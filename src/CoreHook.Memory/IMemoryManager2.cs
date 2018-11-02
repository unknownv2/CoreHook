using System;
using System.Diagnostics;

namespace CoreHook.Memory
{
    public interface IMemoryManager2 : IDisposable
    {
        Func<Process, IntPtr, int, bool> FreeMemory { get; set; }

        IntPtr Add(Process process, IntPtr address, bool isFree, int size = 0);

        void FreeAllocations();
    }
}
