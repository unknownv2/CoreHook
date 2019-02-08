using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal class NtHeaders
    {
        internal uint Signature { get; }
        internal FileHeader FileHeader { get; }
        internal OptionalHeader OptionalHeader { get; }

        internal NtHeaders(BinaryReader reader)
        {
            Signature = reader.ReadUInt32();
            FileHeader = new FileHeader(reader);
            OptionalHeader = new OptionalHeader(reader);
        }
    }
}
