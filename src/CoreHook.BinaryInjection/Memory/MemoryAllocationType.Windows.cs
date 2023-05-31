
using Windows.Win32.System.Memory;

namespace CoreHook.BinaryInjection.Memory;

public enum MemoryAllocationType : uint
{
    Commit = VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT,
    Reserve = VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
    Decommit = VIRTUAL_ALLOCATION_TYPE.MEM_REPLACE_PLACEHOLDER,
    Release = VIRTUAL_FREE_TYPE.MEM_RELEASE,// Interop.Kernel32.MemoryAllocationType.Release,
    Reset = VIRTUAL_ALLOCATION_TYPE.MEM_RESET,
    //Physical = Interop.Kernel32.MemoryAllocationType.Physical,
    //TopDown = Interop.Kernel32.MemoryAllocationType.TopDown,
    //WriteWatch = Interop.Kernel32.MemoryAllocationType.WriteWatch,
    LargePages = VIRTUAL_ALLOCATION_TYPE.MEM_LARGE_PAGES


    //internal const uint Commit = 0x1000;
    //internal const uint Reserve = 0x2000;
    //internal const uint Decommit = 0x4000;
    //internal const uint Release = 0x8000;
    //internal const uint Reset = 0x80000;
    //internal const uint Physical = 0x400000;
    //internal const uint TopDown = 0x100000;
    //internal const uint WriteWatch = 0x200000;
    //internal const uint LargePages = 0x20000000;
}
