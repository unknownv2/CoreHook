using System.Runtime.InteropServices;

namespace CoreHook.BinaryInjection.PortableExecutable;

[StructLayout(LayoutKind.Sequential)]
internal struct FileHeader
{
    [MarshalAs(UnmanagedType.U2)]
    internal FileHeaderMachine Machine;
    internal ushort NumberOfSections;
    internal uint TimeDateStamp;
    internal uint PointerToSymbolTable;
    internal uint NumberOfSymbols;
    internal ushort SizeOfOptionalHeader;
    internal ushort Characteristics;
}
