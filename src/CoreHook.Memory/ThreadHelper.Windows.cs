using System;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using CoreHook.Memory.Formats.PortableExecutable;
using Microsoft.Win32.SafeHandles;

namespace CoreHook.Memory
{
    public static partial class ThreadHelper
    {
        public static SafeWaitHandle CreateRemoteThread(
            SafeProcessHandle processHandle,
            IntPtr startAddress,
            IntPtr parameter)
        {
            return Interop.Kernel32.CreateRemoteThread(
                       processHandle,
                       IntPtr.Zero,
                       UIntPtr.Zero,
                       startAddress,
                       parameter,
                       0,
                       IntPtr.Zero);
        }

        public static IntPtr GetProcAddress(SafeProcessHandle processHandle, string module, string function)
        {
            var moduleHandle = GetModuleHandle(processHandle, module);

            if(moduleHandle == IntPtr.Zero)
            {
                throw new Win32Exception($"Failed to get the {module} handle.");
            }

            return GetModuleFunctionAddress(processHandle, moduleHandle, function);
        }

        private static IntPtr GetModuleFunctionAddress(SafeProcessHandle processHandle, IntPtr moduleHandle, string functionName)
        {
            Interop.Kernel32.NtModuleInfo moduleInfo = GetModuleInfo(processHandle, moduleHandle);

            DataDirectory exportDirectory = 
                GetExportDataDirectory(ReadPage(processHandle, moduleInfo.BaseOfDll), ImageDirectoryEntry.ImageDirectoryEntryExport);

            // Read and parse the export directory from the module.
            var exportTableAddress = moduleInfo.BaseOfDll + (int)exportDirectory.VirtualAddress;
            var exportTable = ReadPage(processHandle, exportTableAddress, (int)exportDirectory.Size);

            return new IntPtr(moduleInfo.BaseOfDll.ToInt64() +
                GetFunctionAddressFromExportDirectory(exportTable, exportDirectory.VirtualAddress, functionName).ToInt64());
        }

        private static byte[] ReadPage(SafeProcessHandle processHandle, IntPtr pageAddress, int? pageSize = null)
        {
            var page = new byte[pageSize ?? Environment.SystemPageSize];
            if(!Interop.Kernel32.ReadProcessMemory(
                processHandle,
                pageAddress,
                page,
                new UIntPtr((uint)page.Length),
                out UIntPtr bytesRead) || bytesRead.ToUInt32() != page.Length)
            {
                throw new Win32Exception(
                    $"Failed to read process memory at {pageAddress.ToInt64():X16}.");
            }
            return page;
        }

        private static Interop.Kernel32.NtModuleInfo GetModuleInfo(SafeProcessHandle processHandle, IntPtr moduleHandle)
        {
            if(!Interop.Kernel32.GetModuleInformation(
                processHandle,
                moduleHandle,
                out Interop.Kernel32.NtModuleInfo moduleInfo))
            {
                throw new Win32Exception("Failed to get process module information");
            }
            return moduleInfo;
        }

        private static IntPtr GetModuleHandle(SafeProcessHandle processHandle, string moduleName)
        {
            IntPtr[] moduleHandles = GetProcessModuleHandles(processHandle);
            char[] chars = new char[1024];

            foreach (IntPtr moduleHandle in moduleHandles)
            {
                int length = Interop.Kernel32.GetModuleFileNameEx(processHandle, moduleHandle, chars, chars.Length);
                if (length == 0)
                {
                    continue;
                }

                var moduleFileName = (length >= 4 && chars[0] == '\\' && chars[1] == '\\' && chars[2] == '?' && chars[3] == '\\') ?
                        new string(chars, 4, length - 4) :
                        new string(chars, 0, length);

                if (length == moduleName.Length)
                {
                    if (moduleName.Equals(moduleFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return moduleHandle;
                    }
                }
            }

            return IntPtr.Zero;
        }

        private static IntPtr[] GetProcessModuleHandles(SafeProcessHandle processHandle)
        {
            IntPtr[] moduleHandles = new IntPtr[64];
            GCHandle moduleHandlesArrayHandle = new GCHandle();
            int moduleCount = 0;
            for (; ;)
            {
                bool enumResult = false;
                try
                {
                    moduleHandlesArrayHandle = GCHandle.Alloc(moduleHandles, GCHandleType.Pinned);
                    // Attempt an arbitrary amount of times since EnumProcessModulesEx can fail
                    // as a result of regular OS operations.
                    for (int i = 0; i < 50; i++)
                    {
                        enumResult = Interop.Psapi.EnumProcessModulesEx(processHandle,
                            moduleHandlesArrayHandle.AddrOfPinnedObject(),
                            (uint) (moduleHandles.Length * IntPtr.Size),
                            ref moduleCount,
                            Interop.Psapi.ModuleFilterFlags.All);
                        if (enumResult)
                        {
                            break;
                        }
                        Thread.Sleep(1);
                    }
                }
                finally
                {
                    moduleHandlesArrayHandle.Free();
                }

                if (!enumResult)
                {
                    throw new Win32Exception("Failed to retrieve process modules.");
                }

                moduleCount /= IntPtr.Size;
                if (moduleCount <= moduleHandles.Length)
                    break;

                moduleHandles = new IntPtr[moduleHandles.Length * 2];
            }
            return moduleHandles;
        }

        private static DataDirectory GetExportDataDirectory(byte[] programHeader, ImageDirectoryEntry directoryEntry)
        {
            using (var reader = new BinaryReader(new MemoryStream(programHeader)))
            {
                var dosHeader = new DosHeader(reader);
                reader.BaseStream.Position = dosHeader.e_lfanew;
                var ntHeaders = new NtHeaders(reader);

                return ntHeaders.OptionalHeader.GetDataDirectory(directoryEntry);
            }
        }

        private static IntPtr GetFunctionAddressFromExportDirectory(byte[] exportDirectoryBuffer, uint exportTableRva, string functionName)
        {
            uint RvaToDirectoryPosition(uint address) => address - exportTableRva;

            using (var reader = new BinaryReader(new MemoryStream(exportDirectoryBuffer)))
            {
                var exportDirectory = new ExportDirectory(reader);

                var functionAddress = IntPtr.Zero;
                var exportFunctionOffset = RvaToDirectoryPosition(exportDirectory.AddressOfFunctions);
                var ordinalOffset = RvaToDirectoryPosition(exportDirectory.AddressOfNameOrdinals);
                var exportNameOffset = RvaToDirectoryPosition(exportDirectory.AddressOfNames);

                reader.BaseStream.Position = exportNameOffset;
                for(var i = 0; i < exportDirectory.NumberOfNames; ++i)
                {
                    reader.BaseStream.Position = exportNameOffset + (i * sizeof(uint));

                    var nameOffset = reader.ReadUInt32();
                    if(nameOffset == 0)
                    {
                        continue;
                    }

                    reader.BaseStream.Position = RvaToDirectoryPosition(nameOffset);
                    if(functionName == ReadAsciiString(reader))
                    {
                        reader.BaseStream.Position = ordinalOffset + (i * sizeof(ushort));
                        var ordinalIndex = reader.ReadUInt16();
                        if(ordinalIndex >= exportDirectory.NumberOfFunctions)
                        {
                            throw new Win32Exception(
                                $"Function ordinal out of range {ordinalIndex} >= {exportDirectory.NumberOfFunctions}");
                        }

                        reader.BaseStream.Position = exportFunctionOffset + (ordinalIndex * sizeof(uint));
                        functionAddress = new IntPtr(reader.ReadUInt32());
                    }
                }
                return functionAddress;
            }
        }

        private static string ReadAsciiString(BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();

            char character;
            while((character = reader.ReadChar()) != '\0')
            {
                stringBuilder.Append(character);
            }

            return stringBuilder.ToString();
        }
    }
}
