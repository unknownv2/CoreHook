
namespace CoreHook.Memory
{
    public partial class MemoryProtectionType
    {
        public const uint Execute = Interop.Kernel32.MemoryProtection.Execute;
        public const uint ExecuteRead = Interop.Kernel32.MemoryProtection.ExecuteRead;
        public const uint ExecuteReadWrite = Interop.Kernel32.MemoryProtection.ExecuteReadWrite;
        public const uint ExecuteWriteCopy = Interop.Kernel32.MemoryProtection.ExecuteWriteCopy;
        public const uint NoAccess = Interop.Kernel32.MemoryProtection.NoAccess;
        public const uint ReadOnly = Interop.Kernel32.MemoryProtection.ReadOnly;
        public const uint ReadWrite = Interop.Kernel32.MemoryProtection.ReadWrite;
        public const uint WriteCopy = Interop.Kernel32.MemoryProtection.WriteCopy;
        public const uint GuardModifierflag = Interop.Kernel32.MemoryProtection.GuardModifierflag;
        public const uint NoCacheModifierflag = Interop.Kernel32.MemoryProtection.NoCacheModifierflag;
        public const uint WriteCombineModifierflag = Interop.Kernel32.MemoryProtection.WriteCombineModifierflag;
    }
}
