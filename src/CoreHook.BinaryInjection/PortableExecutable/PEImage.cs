using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

using Windows.Win32;

namespace CoreHook.BinaryInjection.PortableExecutable;
public class PEImage
{
    private SafeHandle SafeHandle;
    private nint baseAddress;

    private ExportDirectory exportDirectory;

    public static PEImage CreateForProcessModule(SafeHandle handle, nint moduleBaseAddress)
    {
        PEImage ret = new()
        {
            SafeHandle = handle,
            baseAddress = moduleBaseAddress
        };

        ret.Init(); 
        
        return ret;
    }

    private unsafe void Init()
    {
        // TODO: Trying to use the Unsafe API, but it might cause some issues if alignment is not the same depending on the platform
        // since no marshalling occurs here. I've to double check that.
        var dosHeaders = Read<DosHeader>((void*)baseAddress);

        var ntHeadersAddress = baseAddress + dosHeaders.e_lfanew;
        var ntHeaders = Read<NtHeaders>((void*)ntHeadersAddress);

        var exportDirectoryAddress = baseAddress + ntHeaders.OptionalHeader.DataDirectory(ImageDirectoryEntry.ImageDirectoryEntryExport).VirtualAddress;
        exportDirectory = Read<ExportDirectory>((void*)exportDirectoryAddress);
    }

    public unsafe IList<(string Name, nint Address)> GetExportedFunctions()
    {
        var names = new uint[exportDirectory.NumberOfNames];
        var ordinals = new ushort[exportDirectory.NumberOfNames];
        var addresses = new uint[exportDirectory.NumberOfFunctions];
        fixed (uint* namesPtr = names)
        fixed (ushort* ordinalsPtr = ordinals)
        fixed (uint* addressesPtr = addresses)
        {
            Read((void*)(baseAddress + (nint)exportDirectory.AddressOfNames), namesPtr, exportDirectory.NumberOfNames * sizeof(uint));
            Read((void*)(baseAddress + (nint)exportDirectory.AddressOfNameOrdinals), ordinalsPtr, exportDirectory.NumberOfNames * sizeof(ushort));
            Read((void*)(baseAddress + (nint)exportDirectory.AddressOfFunctions), addressesPtr, exportDirectory.NumberOfFunctions * sizeof(uint));
        }

        return names.Zip(ordinals.Select(ordinal => addresses[ordinal]), (nameAddress, functionAddress) => (ReadString(baseAddress + (nint)nameAddress), baseAddress + (nint)functionAddress))
                    .ToList();
    }

    private unsafe string ReadString(nint address)
    {
        // We have to copy the memory buffer using marshalling (hance the indirect PtrToStringAnsi call).
        // Maybe use CopyMemory and release the copied data next?
        fixed (sbyte* bufferPtr = new sbyte[4096])
        {
            Read((void*)address, bufferPtr, 4096);
            // TODO: Could probably use a ReadOnlySpan here; stopping at the first null char and avoiding additional allocation?
            return Marshal.PtrToStringAnsi((nint)bufferPtr);
        }
    }

    private unsafe T Read<T>(void* address, uint? len = null) where T : new()
    {
        var ret = new T();
        len ??= (uint)sizeof(T);

        var bytesRead = new long();
        if (!NativeMethods.ReadProcessMemory(SafeHandle, address, &ret, (nuint)len, (nuint*)&bytesRead) || bytesRead != len)
        {
            throw new Win32Exception($"Failed to read process memory at {(nuint)address:X16}.");
        }

        return ret;
    }

    private unsafe void Read(void* address, void* buffer, uint? len)
    {
        var bytesRead = new long();
        if (!NativeMethods.ReadProcessMemory(SafeHandle, address, buffer, (nuint)len, (nuint*)&bytesRead) || bytesRead != len)
        {
            throw new Win32Exception($"Failed to read process memory at {(nuint)address:X16}.");
        }
    }

}
