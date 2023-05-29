using System.Runtime.InteropServices;

namespace CoreHook.BinaryInjection.PortableExecutable;

[StructLayout(LayoutKind.Sequential)]
internal unsafe readonly struct DataDirectory
{
    internal readonly uint VirtualAddress;

    internal readonly uint Size;

    //internal DataDirectory(BinaryReader reader, int offset)
    //{
    //    reader.BaseStream.Position = offset;
    //    VirtualAddress = reader.ReadUInt32();
    //    Size = reader.ReadUInt32();
    //}
}
