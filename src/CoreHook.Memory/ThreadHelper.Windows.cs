using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CoreHook.Memory
{
    public static partial class ThreadHelper
    {
        public static SafeWaitHandle CreateRemoteThread(SafeProcessHandle processHandle, IntPtr startAddress,
            IntPtr parameter)
        {
            throw new NotImplementedException();
        }
        private static IntPtr GetProcAddress(SafeProcessHandle processHandle, string module, string function)
        {
            var moduleHandle = GetModuleHandle(processHandle, module);

            if(moduleHandle == IntPtr.Zero)
            {
                throw new Win32Exception($"Failed to get the {module} handle.");
            }

            return GetModuleFunctionAddress(processHandle, moduleHandle, function);
        }

        private static IntPtr GetModuleFunctionAddress(SafeProcessHandle processHandle, 
            IntPtr moduleHandle, string functionName)
        {
            Interop.Kernel32.NtModuleInfo moduleInfo = GetModuleInfo(processHandle, moduleHandle);

            DataDirectory exportDirectory = ReadExportDataDirectory(ReadPage(processHandle, moduleInfo.BaseOfDll), 0);

            var exportTable = new byte[exportDirectory.Size];
            var exportTableAddress = moduleInfo.BaseOfDll + (int)exportDirectory.Rva;
            if (!Interop.Kernel32.ReadProcessMemory(
                processHandle,
                exportTableAddress,
                exportTable,
                new UIntPtr((uint)exportTable.Length),
                out UIntPtr bytesRead) || bytesRead.ToUInt32() != exportTable.Length)
            {
                throw new Win32Exception($"Cannot read export table at {exportTableAddress.ToInt64()}");
            }

            processHandle.Dispose();

            return new IntPtr(moduleInfo.BaseOfDll.ToInt64() +
                GetAddressFromExportTable(exportTable, exportDirectory.Rva, functionName).ToInt64());
        }

        private static byte[] ReadPage(SafeProcessHandle processHandle, IntPtr pageAddress)
        {
            var page = new byte[Environment.SystemPageSize];
            if(!Interop.Kernel32.ReadProcessMemory(
                processHandle,
                pageAddress,
                page,
                new UIntPtr((uint)page.Length),
                out UIntPtr bytesRead) || bytesRead.ToUInt32() != page.Length)
            {
                throw new Win32Exception(
                    $"Failed to read process memory page: {pageAddress.ToInt64().ToString("X")}.");
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

                Debug.WriteLine(moduleFileName);
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
                    enumResult = Interop.Psapi.EnumProcessModulesEx(processHandle,
                        moduleHandlesArrayHandle.AddrOfPinnedObject(),
                        (uint)(moduleHandles.Length * IntPtr.Size),
                        ref moduleCount,
                        Interop.Psapi.ModuleFilterFlags.All);
            
                    /*enumResult = Interop.Kernel32.EnumProcessModules(processHandle,
                        moduleHandlesArrayHandle.AddrOfPinnedObject(),
                        moduleHandles.Length * IntPtr.Size, ref moduleCount);*/

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

        private static DataDirectory ReadExportDataDirectory(byte[] programHeader, int index)
        {
            using (var reader = new BinaryReader(new MemoryStream(programHeader)))
            {
                const uint portableExecutableMagic = 0x00004550;
                reader.BaseStream.Position = 0x3c;

                reader.BaseStream.Position = reader.ReadInt32();

                if(reader.ReadUInt32() != portableExecutableMagic)
                {
                    throw new Win32Exception(
                        $"Invalid portable executable header {portableExecutableMagic}.");
                }

                reader.BaseStream.Position += 0x14;

                switch(reader.ReadUInt16())
                {
                    case 0x10:
                        reader.BaseStream.Position += 0x5E;
                        break;
                    case 0x20:
                        reader.BaseStream.Position += 0x6E;
                        break;

                    default:
                        throw new InvalidOperationException("Portable executable header not supported");
                }

                reader.BaseStream.Position += (index * 8);

                var rva = reader.ReadUInt32();
                var size = reader.ReadUInt32();

                reader.Close();

                return new DataDirectory(rva, size);
            }
        }
        private static IntPtr GetAddressFromExportTable(byte[] exportTable, uint exportTableRva, string functionName)
        {
            throw new NotImplementedException();
        }

        internal struct DataDirectory
        {
            internal readonly uint Rva;
            internal readonly uint Size;

            internal DataDirectory(uint rva, uint size)
            {
                Rva = rva;
                Size = size;
            }
        }
    }
}
