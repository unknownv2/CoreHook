using System;

namespace CoreHook.Memory
{
    public interface IMemoryManager : IMemoryAllocator, IMemoryReader, IMemoryWriter, IDisposable
    {
    }
}
