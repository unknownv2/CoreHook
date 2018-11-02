
internal partial class Interop
{
    public partial class MemoryProtection
    {
        public const uint Execute = 0x10;
        public const uint ExecuteRead = 0x20;
        public const uint ExecuteReadWrite = 0x40;
        public const uint ExecuteWriteCopy = 0x80;
        public const uint NoAccess = 0x01;
        public const uint ReadOnly = 0x02;
        public const uint ReadWrite = 0x04;
        public const uint WriteCopy = 0x08;
        public const uint GuardModifierflag = 0x100;
        public const uint NoCacheModifierflag = 0x200;
        public const uint WriteCombineModifierflag = 0x400;
    }
}
