using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal class OptionalHeader
    {
        internal DataDirectory[] DataDirectory { get; }
        private const int DirectoryEntryCount = 16;

        internal OptionalHeader(BinaryReader reader, bool is64Bit)
        {
            int offset = (int)reader.BaseStream.Position;

            DataDirectory = new DataDirectory[DirectoryEntryCount];
            for (int i = 0; i < DirectoryEntryCount; i++)
            {
                if (!is64Bit)
                {
                    DataDirectory[i] = new DataDirectory(reader, offset + 0x60 + i * 0x08);
                }
                else
                {
                    DataDirectory[i] = new DataDirectory(reader, offset + 0x70 + i * 0x08);
                }
            }
        }

        internal DataDirectory GetDataDirectory(ImageDirectoryEntry entry) => DataDirectory[(int) entry];
    }
}
