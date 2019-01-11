using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal class NtHeaders
    {
        internal uint Signature;
        internal FileHeader FileHeader;
        internal OptionalHeader OptionalHeader;

        internal NtHeaders(BinaryReader reader)
        {
            Signature = reader.ReadUInt32();
            FileHeader = new FileHeader(reader);
            OptionalHeader = new OptionalHeader(reader, FileHeader.Is64Bit);
        }
    }
}
