
internal partial class Interop
{
    internal partial class MemoryAllocationType
    {
        internal const uint Commit = 0x1000;
        internal const uint Reserve = 0x2000;
        internal const uint Decommit = 0x4000;
        internal const uint Release = 0x8000;
        internal const uint Reset = 0x80000;
        internal const uint Physical = 0x400000;
        internal const uint TopDown = 0x100000;
        internal const uint WriteWatch = 0x200000;
        internal const uint LargePages = 0x20000000;
    }
}
