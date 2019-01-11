
namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal enum FileHeaderMachine
    {
        I386 = 0x14c,   // Intel 386 (32-bit)
        ARMNT = 0x1c4,  // ARM Thumb-2 Little-Endian (32-bit)
        AMD64 = 0x8664, // AMD64 (64-bit)
        ARM64 = 0xaa64  // ARM64 Little-Endian (64-bit)
    }
}
