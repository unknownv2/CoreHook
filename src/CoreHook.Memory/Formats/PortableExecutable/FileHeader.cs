using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal class FileHeader
    {
        internal FileHeaderMachine Machine { get; }
        internal ushort NumberOfSections { get; }
        internal uint TimeDateStamp { get; }
        internal uint PointerToSymbolTable { get; }
        internal uint NumberOfSymbols { get; }
        internal ushort SizeOfOptionalHeader { get; }
        internal ushort Characteristics { get; }

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
    }
}
