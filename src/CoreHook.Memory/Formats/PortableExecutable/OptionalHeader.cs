using System.IO;

namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal class OptionalHeader
    {
        internal DataDirectory[] DataDirectory { get; }
        internal ImageMagic ImageMagic { get; }

        private const int DirectoryEntryCount = 16;

        internal OptionalHeader(BinaryReader reader)
        {
            int offset = (int)reader.BaseStream.Position;

            // Read the image type (either PE32 or PE32+)
            ImageMagic = (ImageMagic)reader.ReadUInt16();

            DataDirectory = new DataDirectory[DirectoryEntryCount];
            for (int i = 0; i < DirectoryEntryCount; i++)
            {
                if (ImageMagic == ImageMagic.Magic32)
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
