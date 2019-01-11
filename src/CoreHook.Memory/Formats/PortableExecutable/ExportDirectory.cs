using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal class ExportDirectory
    {
        internal uint Characteristics { get; }
        internal uint TimeDateStamp { get; }
        internal ushort MajorVersion { get; }
        internal ushort MinorVersion { get; }
        internal uint Name { get; }
        internal uint Base { get; }
        internal uint NumberOfFunctions { get; }
        internal uint NumberOfNames { get; }
        internal uint AddressOfFunctions { get; }
        internal uint AddressOfNames { get; }
        internal uint AddressOfNameOrdinals { get; }

        internal ExportDirectory(BinaryReader reader)
        {
            Characteristics = reader.ReadUInt32();
            TimeDateStamp = reader.ReadUInt32();
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            Name = reader.ReadUInt32();
            Base = reader.ReadUInt32();
            NumberOfFunctions = reader.ReadUInt32();
            NumberOfNames = reader.ReadUInt32();
            AddressOfFunctions = reader.ReadUInt32();
            AddressOfNames = reader.ReadUInt32();
            AddressOfNameOrdinals = reader.ReadUInt32();
        }
    }
}
