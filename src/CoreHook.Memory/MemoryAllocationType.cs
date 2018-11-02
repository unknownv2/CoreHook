
namespace CoreHook.Memory
{
    public partial class MemoryAllocationType
    {
        public const uint Commit = Interop.Kernel32.MemoryAllocationType.Commit;
        public const uint Reserve = Interop.Kernel32.MemoryAllocationType.Reserve;
    }
}
