using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal class FileHeader
    {
        internal FileHeaderMachine Machine;
        internal ushort NumberOfSections;
        internal uint TimeDateStamp;
        internal uint PointerToSymbolTable;
        internal uint NumberOfSymbols;
        internal ushort SizeOfOptionalHeader;
        internal ushort Characteristics;

        internal FileHeader(BinaryReader reader)
        {
            Machine = (FileHeaderMachine)reader.ReadUInt16();
            NumberOfSections = reader.ReadUInt16();
            TimeDateStamp = reader.ReadUInt32();
            PointerToSymbolTable = reader.ReadUInt32();
            NumberOfSymbols = reader.ReadUInt32();
            SizeOfOptionalHeader = reader.ReadUInt16();
            Characteristics = reader.ReadUInt16();
        }

        internal bool Is64Bit => Machine == FileHeaderMachine.AMD64 || Machine == FileHeaderMachine.ARM64;
    }
}
