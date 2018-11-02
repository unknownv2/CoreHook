
namespace CoreHook.Memory
{
    public partial class MemoryAllocationType
    {
        public const uint Commit = Interop.Kernel32.MemoryAllocationType.Commit;
        public const uint Reserve = Interop.Kernel32.MemoryAllocationType.Reserve;
        public const uint Decommit = Interop.Kernel32.MemoryAllocationType.Decommit;
        public const uint Release = Interop.Kernel32.MemoryAllocationType.Release;
        public const uint Reset = Interop.Kernel32.MemoryAllocationType.Reset;
        public const uint Physical = Interop.Kernel32.MemoryAllocationType.Physical;
        public const uint TopDown = Interop.Kernel32.MemoryAllocationType.TopDown;
        public const uint WriteWatch = Interop.Kernel32.MemoryAllocationType.WriteWatch;
        public const uint LargePages = Interop.Kernel32.MemoryAllocationType.LargePages;
    }
}
