using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal struct DataDirectory
    {
        internal uint VirtualAddress;
        internal uint Size;

        internal DataDirectory(BinaryReader reader, int offset)
        {
            reader.BaseStream.Position = offset;
            VirtualAddress = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }
    }
}
