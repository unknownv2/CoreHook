using System.Runtime.InteropServices;

namespace CoreHook.BinaryInjection.PortableExecutable;

[StructLayout(LayoutKind.Sequential)]
internal struct NtHeaders
{
    internal uint Signature;
    internal FileHeader FileHeader;
    internal OptionalHeader OptionalHeader;
}
