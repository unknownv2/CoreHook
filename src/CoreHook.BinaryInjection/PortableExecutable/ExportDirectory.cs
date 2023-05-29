using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace CoreHook.BinaryInjection.PortableExecutable;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ExportDirectory
{
    internal readonly uint Characteristics;
    internal readonly uint TimeDateStamp;
    internal readonly ushort MajorVersion;
    internal readonly ushort MinorVersion;
    internal readonly uint Name;
    internal readonly uint Base;
    internal readonly uint NumberOfFunctions;
    internal readonly uint NumberOfNames;
    internal readonly uint AddressOfFunctions;
    internal readonly uint AddressOfNames;
    internal readonly uint AddressOfNameOrdinals;
    
    //internal unsafe IList<(string? Name, nint Address)> RetrieveFunctions(nint baseAddress)
    //{
    //    fixed (byte* t = new byte[NumberOfNames])

    //    var names = new ReadOnlySpan<uint>((void*)(baseAddress + (nint)AddressOfNames), (int)NumberOfNames).ToArray();
        
    //    var ordinals = new ReadOnlySpan<ushort>((void*)(baseAddress + (nint)AddressOfNameOrdinals), (int)NumberOfNames).ToArray();

    //    var addresses = new ReadOnlySpan<uint>((void*)(baseAddress + (nint)AddressOfFunctions), (int)NumberOfFunctions).ToArray();

    //    var ret = names.Zip(ordinals.Select(ordinal => addresses[ordinal]), (nameAddress, functionAddress) => (Marshal.PtrToStringAnsi(baseAddress + (nint)nameAddress), baseAddress + (nint)functionAddress)).ToList();
    //    return ret;
    //}

}
