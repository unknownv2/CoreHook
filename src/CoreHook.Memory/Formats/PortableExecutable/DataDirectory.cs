using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal struct DataDirectory
    {
        internal uint VirtualAddress { get; }
        internal uint Size { get; }

        internal DataDirectory(BinaryReader reader, int offset)
        {
            reader.BaseStream.Position = offset;
            VirtualAddress = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }
    }
}
