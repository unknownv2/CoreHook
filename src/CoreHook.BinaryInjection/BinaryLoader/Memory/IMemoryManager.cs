using System;
using System.Diagnostics;

namespace CoreHook.BinaryInjection.BinaryLoader.Memory
{
    public interface IMemoryManager : IDisposable
    {
        Func<Process, IntPtr, uint, bool> FreeMemory { get; set; }

        IntPtr Add(Process process, IntPtr address, bool isFree, uint size = 0);

        void FreeAllocations();
    }
}
