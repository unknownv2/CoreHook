using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.Unmanaged
{
    public static class ProcessExtensions
    {
        private static readonly Dictionary<string, IntPtr> CachedWow64Addresses = new Dictionary<string, IntPtr>();

        private static IntPtr GetHandle(int processId, NativeMethods.ProcessAccessFlags accessFlags)
        {
            var handle = NativeMethods.OpenProcess(accessFlags, false, processId);

            if (handle == IntPtr.Zero)
            {
                throw new UnauthorizedAccessException("Failed to open process with query access.");
            }

            return handle;
        }

        private static IntPtr GetReadHandle(int processId)
        {
            return GetHandle(processId,
                NativeMethods.ProcessAccessFlags.QueryInformation 
                | NativeMethods.ProcessAccessFlags.VirtualMemoryRead);
        }

        public static bool BringToFront(this Process process)
        {
            if (IsActiveWindow(process))
            {
                return true;
            }

            var hWindow = process.MainWindowHandle;

            var style = NativeMethods.GetWindowLong(hWindow, NativeMethods.GWL_STYLE);

            if ((style & NativeMethods.WS_MINIMIZE) != 0)
            {
                NativeMethods.ShowWindow(hWindow, NativeMethods.ShowWindowCommand.Normal);
            }

            return NativeMethods.BringWindowToTop(process.MainWindowHandle);
        }

        public static bool IsActiveWindow(this Process process)
        {
            if (process.HasExited)
            {
                return false;
            }

            var activeWindow = NativeMethods.GetForegroundWindow();

            if (activeWindow.Equals(IntPtr.Zero))
            {
                return false;
            }

            uint pid;

            NativeMethods.GetWindowThreadProcessId(activeWindow, out pid);

            return Marshal.GetLastWin32Error() == 0 && pid == process.Id;
        }

        public static IntPtr GetWin32ProcAddress(this Process process, string module, string function)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                if (process.Is64Bit())
                {
                    if (!Environment.Is64BitProcess)
                    {
                        throw new InvalidOperationException("Cannot get 64-bit proc address from a 32-bit process.");
                    }
                }
            }

            var hModule = NativeMethods.GetModuleHandle(module);

            return NativeMethods.GetProcAddress(hModule, function);
        }

        public static void LoadLibrary(this Process process, string modulePath)
        {
            using (var hProcess = SafeHandle.Wrap(GetHandle(process.Id,
                NativeMethods.ProcessAccessFlags.CreateThread |
                NativeMethods.ProcessAccessFlags.QueryInformation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryOperation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryRead |
                NativeMethods.ProcessAccessFlags.VirtualMemoryWrite)))
            {
                var pathBytes = Encoding.Unicode.GetBytes(modulePath + "\0");

                // Allocate space in the remote process for the DLL path. 
                var remoteAllocAddr = NativeMethods.VirtualAllocEx(
                    hProcess.Handle,
                    IntPtr.Zero,
                    (uint)pathBytes.Length,
                    NativeMethods.AllocationType.Commit | NativeMethods.AllocationType.Reserve,
                    NativeMethods.MemoryProtection.ReadWrite);

                if (remoteAllocAddr == IntPtr.Zero)
                {
                    throw new Win32Exception("Failed to allocate memory in remote process.");
                }

                try
                {
                    UIntPtr bytesWritten;

                    // Write the DLL path to the allocated memory.
                    var result = NativeMethods.WriteProcessMemory(
                        hProcess.Handle,
                        remoteAllocAddr,
                        pathBytes,
                        (uint)pathBytes.Length,
                        out bytesWritten);

                    if (!result || bytesWritten.ToUInt32() != pathBytes.Length)
                    {
                        throw new Win32Exception("Failed to allocate memory in remote process.");
                    }

                    // Create a thread in the process at LoadLibraryW and pass it the DLL path.
                    var hThread = NativeMethods.CreateRemoteThread(
                        hProcess.Handle,
                        IntPtr.Zero,
                        0,
                        process.GetWin32ProcAddress("kernel32", "LoadLibraryW"),
                        remoteAllocAddr,
                        0,
                        IntPtr.Zero);

                    if (hThread == IntPtr.Zero)
                    {
                        throw new Win32Exception("Failed to create thread in remote process.");
                    }

                    NativeMethods.WaitForSingleObject(hThread, NativeMethods.INFINITE);

                    // We don't need this handle.
                    NativeMethods.CloseHandle(hThread);
                }
                finally
                {
                    NativeMethods.VirtualFreeEx(hProcess.Handle, remoteAllocAddr, pathBytes.Length, NativeMethods.FreeType.Release);
                }
            }
        }
        /// <summary>
        /// Execute an exported function inside the specified module with custom arguments
        /// </summary>
        /// <param name="process">A handle to the main process with the target module loaded.</param>
        /// <param name="module">The name of the module containing the desired function.</param>
        /// <param name="function">The name of the exported function we will call.</param>
        /// <param name="args">Serialized arguments for passing to the module function.</param>
        public static void ExecuteByModule(this Process process, string module, string function, byte[] args)
        {
            using (var hProcess = SafeHandle.Wrap(GetHandle(process.Id,
                NativeMethods.ProcessAccessFlags.CreateThread |
                NativeMethods.ProcessAccessFlags.QueryInformation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryOperation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryRead |
                NativeMethods.ProcessAccessFlags.VirtualMemoryWrite)))
            {

                // Allocate space in the remote process for the DLL path. 
                var remoteAllocAddr = NativeMethods.VirtualAllocEx(
                    hProcess.Handle,
                    IntPtr.Zero,
                    (uint)args.Length,
                    NativeMethods.AllocationType.Commit | NativeMethods.AllocationType.Reserve,
                    NativeMethods.MemoryProtection.ReadWrite);

                if (remoteAllocAddr == IntPtr.Zero)
                {
                    throw new Win32Exception("Failed to allocate memory in remote process.");
                }

                try
                {
                    UIntPtr bytesWritten;

                    // Write the DLL path to the allocated memory.
                    var result = NativeMethods.WriteProcessMemory(
                        hProcess.Handle,
                        remoteAllocAddr,
                        args,
                        (uint)args.Length,
                        out bytesWritten);

                    if (!result || bytesWritten.ToUInt32() != args.Length)
                    {
                        throw new Win32Exception("Failed to allocate memory in remote process.");
                    }

                    //var addr = process.GetAbsoluteFunctionAddress(module, function);
                    // Create a thread in the process at LoadLibraryW and pass it the DLL path.
                    var hThread = NativeMethods.CreateRemoteThread(
                        hProcess.Handle,
                        IntPtr.Zero,
                        0,
                        process.GetAbsoluteFunctionAddress(module, function),
                        remoteAllocAddr,
                        0,
                        IntPtr.Zero);

                    if (hThread == IntPtr.Zero)
                    {
                        throw new Win32Exception("Failed to create thread in remote process.");
                    }

                    // We don't need this handle.
                    NativeMethods.CloseHandle(hThread);
                }
                finally
                {
                    NativeMethods.VirtualFreeEx(hProcess.Handle, remoteAllocAddr, args.Length, NativeMethods.FreeType.Release);
                }
            }
        }
        /// <summary>
        /// Execute an exported function inside the specified module with custom arguments
        /// </summary>
        /// <param name="process">A handle to the main process with the target module loaded.</param>
        /// <param name="module">The name of the module containing the desired function.</param>
        /// <param name="function">The name of the exported function we will call.</param>
        /// <param name="args">Serialized arguments for passing to the module function.</param>
        /// <param name="canWait">We can wait for the thread to finish before cleaning up memory or we need to cleanup later.</param>
        public static IntPtr Execute(this Process process, string module, string function, byte[] args, bool canWait = true)
        {
            using (var hProcess = SafeHandle.Wrap(GetHandle(process.Id,
                NativeMethods.ProcessAccessFlags.CreateThread |
                NativeMethods.ProcessAccessFlags.QueryInformation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryOperation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryRead |
                NativeMethods.ProcessAccessFlags.VirtualMemoryWrite)))
            {

                // Allocate space in the remote process for the DLL path. 
                var remoteAllocAddr = NativeMethods.VirtualAllocEx(
                    hProcess.Handle,
                    IntPtr.Zero,
                    (uint)args.Length,
                    NativeMethods.AllocationType.Commit | NativeMethods.AllocationType.Reserve,
                    NativeMethods.MemoryProtection.ReadWrite);

                if (remoteAllocAddr == IntPtr.Zero)
                {
                    throw new Win32Exception("Failed to allocate memory in remote process.");
                }

                try
                {
                    UIntPtr bytesWritten;

                    // Write the DLL path to the allocated memory.
                    var result = NativeMethods.WriteProcessMemory(
                        hProcess.Handle,
                        remoteAllocAddr,
                        args,
                        (uint)args.Length,
                        out bytesWritten);

                    if (!result || bytesWritten.ToUInt32() != args.Length)
                    {
                        throw new Win32Exception("Failed to allocate memory in remote process.");
                    }

                    //var addr = process.GetAbsoluteFunctionAddress(module, function);
                    // Create a thread in the process at LoadLibraryW and pass it the DLL path.
                    var hThread = NativeMethods.CreateRemoteThread(
                        hProcess.Handle,
                        IntPtr.Zero,
                        0,
                        process.GetAbsoluteFunctionAddressEx(module, function),
                        remoteAllocAddr,
                        0,
                        IntPtr.Zero);

                    if (hThread == IntPtr.Zero)
                    {
                        throw new Win32Exception("Failed to create thread in remote process.");
                    }

                    if (canWait)
                    {
                        NativeMethods.WaitForSingleObject(hThread, NativeMethods.INFINITE);
                    }

                    // We don't need this handle.
                    NativeMethods.CloseHandle(hThread);
                    return remoteAllocAddr;
                }
                finally
                {
                    if (canWait)
                    {
                        NativeMethods.VirtualFreeEx(hProcess.Handle, remoteAllocAddr, 0, NativeMethods.FreeType.Release);
                    }
                }
            }
        }
 
        public static IntPtr MemAllocate(this Process process, uint size)
        {
            using (var hProcess = SafeHandle.Wrap(GetHandle(process.Id,
                NativeMethods.ProcessAccessFlags.QueryInformation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryOperation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryRead |
                NativeMethods.ProcessAccessFlags.VirtualMemoryWrite)))
            {

                // Allocate space in the remote process for the DLL path. 
                var remoteAllocAddr = NativeMethods.VirtualAllocEx(
                    hProcess.Handle,
                    IntPtr.Zero,
                    size,
                    NativeMethods.AllocationType.Commit | NativeMethods.AllocationType.Reserve,
                    NativeMethods.MemoryProtection.ReadWrite);

                if (remoteAllocAddr == IntPtr.Zero)
                {
                    throw new Win32Exception("Failed to allocate memory in remote process.");
                }

                return remoteAllocAddr;
            }
        }

        public static IntPtr MemCopyTo(this Process process, byte[] data, uint size = 0)
        {
            using (var hProcess = SafeHandle.Wrap(GetHandle(process.Id,
                  NativeMethods.ProcessAccessFlags.QueryInformation |
                  NativeMethods.ProcessAccessFlags.VirtualMemoryOperation |
                  NativeMethods.ProcessAccessFlags.VirtualMemoryRead |
                  NativeMethods.ProcessAccessFlags.VirtualMemoryWrite)))
            {
                var dataLen = size > 0 ? size : (uint)data.Length;
                var remoteAllocAddr = MemAllocate(process, dataLen);
                UIntPtr bytesWritten;

                // Write the DLL path to the allocated memory.
                var result = NativeMethods.WriteProcessMemory(
                    hProcess.Handle,
                    remoteAllocAddr,
                    data,
                    dataLen,
                    out bytesWritten);

                if (!result || bytesWritten.ToUInt32() != dataLen)
                {
                    throw new Win32Exception("Failed to allocate memory in remote process.");
                }
                return remoteAllocAddr;
            }
        }

        public static bool FreeMemory(this Process process, IntPtr address, int size = 0)
        {
            if (address == IntPtr.Zero)
                return true;

            using (var hProcess = SafeHandle.Wrap(GetHandle(process.Id,
                  NativeMethods.ProcessAccessFlags.QueryInformation |
                  NativeMethods.ProcessAccessFlags.VirtualMemoryOperation)))
            {
                return size == 0 ?
                    NativeMethods.VirtualFreeEx(hProcess.Handle, address, size, NativeMethods.FreeType.Release)
                    : NativeMethods.VirtualFreeEx(hProcess.Handle, address, size, NativeMethods.FreeType.Decommit);
            }
        }
        public static bool Is64Bit(this Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                return false;
            }

            var handle = NativeMethods.OpenProcess(
                NativeMethods.ProcessAccessFlags.QueryInformation,
                false,
                process.Id
            );

            if (handle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            using (SafeHandle.Wrap(handle))
            {
                bool ret;

                if (!NativeMethods.IsWow64Process(handle, out ret))
                {
                    throw new Win32Exception();
                }

                return !ret;
            }
        }

        public static ExportedFunction[] GetExportedFunctions(this Process process, string moduleBaseName)
        {
            var hProcess = GetReadHandle(process.Id);

            var hModule = GetModuleHandleByBaseName(process, moduleBaseName);

            if (hModule == IntPtr.Zero)
            {
                throw new Win32Exception("Module not found in process.");
            }

            return GetExportedFunctions(hProcess, hModule);
        }

        public static ExportedFunction[] GetExportedFunctions(this Process process, IntPtr hModule)
        {
            return GetExportedFunctions(GetReadHandle(process.Id), hModule);
        }

        public static IntPtr GetAbsoluteFunctionAddress(this Process process, string moduleBaseName, string functionName)
        {
            var hProcess = GetReadHandle(process.Id);

            var hModule = GetModuleHandleByBaseName(hProcess, moduleBaseName);

            if (hModule == IntPtr.Zero)
            {
                throw new ModuleNotFoundException("Module not found in process.");
            }

            return GetAbsoluteFunctionAddress(hProcess, hModule, functionName);
        }
        public static IntPtr GetAbsoluteFunctionAddressEx(this Process process, string moduleFileName, string functionName)
        {
            var hProcess = GetReadHandle(process.Id);

            var hModule = GetModuleHandleByFileName(hProcess, moduleFileName);

            if (hModule == IntPtr.Zero)
            {
                throw new ModuleNotFoundException("Module not found in process.");
            }

            return GetAbsoluteFunctionAddress(hProcess, hModule, functionName);
        }
        public static IntPtr GetAbsoluteFunctionAddress(this Process process, IntPtr hModule, string functionName)
        {
            using (var handle = SafeHandle.Wrap(GetReadHandle(process.Id)))
            {
                return GetAbsoluteFunctionAddress(handle.Handle, hModule, functionName);
            }
        }

        private static IntPtr GetAbsoluteFunctionAddress(IntPtr hProcess, IntPtr hModule, string functionName)
        {
            var moduleInfo = GetModuleInfo(hProcess, hModule);

            var exportDir = GetDataDirectory(ReadPage(hProcess, moduleInfo.BaseAddress), 0);

            var buffer = new byte[exportDir.Size];

            IntPtr bytesRead;

            if (!NativeMethods.ReadProcessMemory(
                hProcess,
                moduleInfo.BaseAddress + (int)exportDir.Rva,
                buffer,
                buffer.Length,
                out bytesRead) || bytesRead != (IntPtr)buffer.Length)
            {
                throw new Win32Exception("Failed to read export table from memory of module.");
            }

            return new IntPtr(moduleInfo.BaseAddress.ToInt64() + GetFunctionAddress(buffer, exportDir.Rva, functionName).ToInt64());
        }

        private static ExportedFunction[] GetExportedFunctions(IntPtr hProcess, IntPtr hModule)
        {
            try
            {
                var moduleInfo = GetModuleInfo(hProcess, hModule);

                var exportDir = GetDataDirectory(ReadPage(hProcess, moduleInfo.BaseAddress), 0);

                var buffer = new byte[exportDir.Size];

                IntPtr bytesRead;

                if (!NativeMethods.ReadProcessMemory(
                    hProcess,
                    moduleInfo.BaseAddress + (int)exportDir.Rva,
                    buffer,
                    buffer.Length,
                    out bytesRead) || bytesRead != (IntPtr)buffer.Length)
                {
                    throw new Win32Exception("Failed to read export table from memory of module.");
                }

                return ParseExportTable(buffer, moduleInfo.BaseAddress.ToInt64());
            }
            finally
            {
                NativeMethods.CloseHandle(hProcess);
            }
        }

        private static NativeMethods.MODULEINFO GetModuleInfo(IntPtr hProcess, IntPtr hModule)
        {
            NativeMethods.MODULEINFO moduleInfo;

            if (!NativeMethods.GetModuleInformation(hProcess, hModule, out moduleInfo, (uint)Marshal.SizeOf<NativeMethods.MODULEINFO>()))
            {
                throw new Win32Exception("Failed to get module information.");
            }

            return moduleInfo;
        }

        private static byte[] ReadPage(IntPtr hProcess, IntPtr address)
        {
            var buffer = new byte[Environment.SystemPageSize];

            IntPtr bytesRead;

            if (!NativeMethods.ReadProcessMemory(
                hProcess,
                address,
                buffer,
                buffer.Length,
                out bytesRead) || bytesRead != (IntPtr)buffer.Length)
            {
                throw new Win32Exception("Failed to read PE header from memory of module.");
            }

            return buffer;
        }

        private static DataDirectory GetDataDirectory(byte[] peHeader, int index)
        {
            using (var io = new BinaryReader(new MemoryStream(peHeader)))
            {
                io.BaseStream.Position = 0x3c;

                // Skip to the PE header.
                io.BaseStream.Position = io.ReadInt32();

                // Check for the PE\0\0 magic.
                if (io.ReadUInt32() != 0x00004550)
                {
                    throw new Win32Exception("Invalid PE header in module.");
                }

                // Skip the COFF header.
                io.BaseStream.Position += 20;

                // Read the optional header magic and skip to the directory listing.
                switch (io.ReadUInt16())
                {
                    // 32-bit application
                    case 0x10b:
                        io.BaseStream.Position += 94;
                        break;

                    // 64-bit application
                    case 0x20b:
                        io.BaseStream.Position += 110;
                        break;

                    // What the hell is it then?
                    default:
                        throw new BadImageFormatException("Unknown optional header magic in PE header.");
                }

                io.BaseStream.Position += 8*index;

                var rva = io.ReadUInt32();

                var dir = new DataDirectory(rva, io.ReadUInt32());

                io.Close();

                return dir;
            }
        }

        private static ExportedFunction[] ParseExportTable(byte[] exportTable, long imageBase)
        {
            var ms = new MemoryStream(exportTable);
            var io = new BinaryReader(ms);

            // Skip flags, timestamp, version, and name of DLL RVA.
            ms.Position = 16;

            // Usually always 1.
            // ReSharper disable once UnusedVariable
            var ordinalBase = io.ReadInt32();

            var addressTableEntryCount = io.ReadUInt32();
            var namePointerTableEntryCount = io.ReadUInt32();
            var exportAddressTableRva = io.ReadUInt32() - imageBase;
            var exportNamePointerTableRva = io.ReadUInt32() - imageBase;
            var ordinalTableRva = io.ReadUInt32() - imageBase;

            var exports = new List<ExportedFunction>((int)namePointerTableEntryCount);

            ms.Position = exportNamePointerTableRva;

            int x;

            // TODO: If performance is bad, change this to a binary search.
            for (x = 0; x < namePointerTableEntryCount; x++)
            {
                ms.Position = exportNamePointerTableRva + (x*4);

                var nameRva = io.ReadUInt32();

                if (nameRva == 0)
                {
                    continue;
                }

                ms.Position = nameRva - imageBase;

                var funcName = ReadNullTerminatedAsciiString(io);

                ms.Position = ordinalTableRva + (x * 2);
                var ordinal = io.ReadUInt16(); // + ordinalBase;

                if (ordinal >= addressTableEntryCount)
                {
                    io.Close();
                    throw new Win32Exception("Corrupted export table in module.");
                }

                ms.Position = exportAddressTableRva + (ordinal * 4); // (ordinal - ordinalBase)

                exports.Add(new ExportedFunction(funcName, (IntPtr) io.ReadUInt32()));
            }

            io.Close();

            return exports.ToArray();
        }

        private static IntPtr GetFunctionAddress(byte[] exportTable, uint exportTableRva, string functionName)
        {
            var ms = new MemoryStream(exportTable);

            using (var io = new BinaryReader(ms))
            {
                // Skip flags, timestamp, version, and name of DLL RVA.
                ms.Position = 16;

                // Usually always 1.
                // ReSharper disable once UnusedVariable
                var ordinalBase = io.ReadInt32();

                var addressTableEntryCount = io.ReadUInt32();
                var namePointerTableEntryCount = io.ReadUInt32();
                var exportAddressTableRva = io.ReadUInt32() - exportTableRva;
                var exportNamePointerTableRva = io.ReadUInt32() - exportTableRva;
                var ordinalTableRva = io.ReadUInt32() - exportTableRva;

                ms.Position = exportNamePointerTableRva;

                var ret = IntPtr.Zero;
                int x;

                // TODO: If performance is bad, change this to a binary search.
                for (x = 0; x < namePointerTableEntryCount; x++)
                {
                    ms.Position = exportNamePointerTableRva + (x * 4);

                    var nameRva = io.ReadUInt32();

                    if (nameRva == 0)
                    {
                        continue;
                    }

                    ms.Position = nameRva - exportTableRva;

                    if (functionName == ReadNullTerminatedAsciiString(io))
                    {
                        ms.Position = ordinalTableRva + (x * 2);
                        var ordinal = io.ReadUInt16(); // + ordinalBase;

                        if (ordinal >= addressTableEntryCount)
                        {
                            throw new Win32Exception("Corrupted export table in module.");
                        }

                        ms.Position = exportAddressTableRva + (ordinal * 4); // (ordinal - ordinalBase)

                        ret = (IntPtr)io.ReadUInt32();
                    }
                }

                return ret;
            }
        }

        private static string ReadNullTerminatedAsciiString(BinaryReader io)
        {
            var sb = new StringBuilder();

            byte b;
            while ((b = io.ReadByte()) != 0x00)
            {
                sb.Append((char)b);
            }

            return sb.ToString();
        }

        /*const int PeHeaderRva = 64;
        const int CoffHeaderRva = PeHeaderRva + 4;
        const int OptionalHeaderRva = CoffHeaderRva + 20;

        [StructLayout(LayoutKind.Explicit)]
        private struct ExportTablePeImage
        {
            [FieldOffset(PeHeaderRva)]
            public uint PeMagic;

            [FieldOffset(OptionalHeaderRva)]
            public ushort OptHeaderMagic;

            [FieldOffset(OptionalHeaderRva + 92)]
            public uint NumberOfRvasAndSizes32;

            [FieldOffset(OptionalHeaderRva + 108)]
            public uint NumberOfRvasAndSizes64;

            [FieldOffset(OptionalHeaderRva + 96)]
            public uint ExportTableDirectoryRva32;

            [FieldOffset(OptionalHeaderRva + 100)]
            public uint ExportTableDirectorySize32;

            [FieldOffset(OptionalHeaderRva + 112)]
            public uint ExportTableDirectoryRva64;

            [FieldOffset(OptionalHeaderRva + 116)]
            public uint ExportTableDirectorySize64;
        }*/

        public static IntPtr GetModuleHandleByBaseName(this Process process, string moduleName)
        {
            using (var handle = SafeHandle.Wrap(GetReadHandle(process.Id)))
            {
                return GetModuleHandleByBaseName(handle.Handle, moduleName);
            }
        }
        public static IntPtr GetModuleHandleByBaseName(IntPtr hProcess, string moduleName)
        {
            var handles = GetAllModuleHandles(hProcess);

            foreach (var moduleHandle in handles)
            {
                var sb = new StringBuilder(256);

                if (NativeMethods.GetModuleBaseName(hProcess, moduleHandle, sb, 512) == moduleName.Length)
                {
                    if (moduleName.Equals(sb.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return moduleHandle;
                    }
                }
            }

            return IntPtr.Zero;
        }
        public static IntPtr GetModuleHandleByFileName(this Process process, string moduleFileName)
        {
            using (var handle = SafeHandle.Wrap(GetReadHandle(process.Id)))
            {
                return GetModuleHandleByFileName(handle.Handle, moduleFileName);
            }
        }
        public static IntPtr GetModuleHandleByFileName(IntPtr hProcess, string moduleName)
        {
            var handles = GetAllModuleHandles(hProcess);

            foreach (var moduleHandle in handles)
            {
                var sb = new StringBuilder(256);

                if (NativeMethods.GetModuleFileNameEx(hProcess, moduleHandle, sb, 512) == moduleName.Length)
                {
                    if (moduleName.Equals(sb.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return moduleHandle;
                    }
                }
                Debug.WriteLine(sb.ToString());
            }

            return IntPtr.Zero;
        }
        public static IntPtr[] GetAllModuleHandles(this Process process)
        {
            var hProcess = GetReadHandle(process.Id);

            try
            {
                return GetAllModuleHandles(hProcess);
            }
            finally
            {
                NativeMethods.CloseHandle(hProcess);
            }
        }

        private static IntPtr[] GetAllModuleHandles(IntPtr hProcess)
        {
            var moduleHandles = new IntPtr[64];

            uint size = 0;

            for (; ; )
            {
                var gcHandle = GCHandle.Alloc(moduleHandles, GCHandleType.Pinned);

                int x;
                var success = false;

                // Try 50 times because it can fail during normal operation.
                for (x = 0; x < 50 && !success; x++)
                {
                    success = NativeMethods.EnumProcessModulesEx(
                        hProcess,
                        gcHandle.AddrOfPinnedObject(),
                        (uint)(IntPtr.Size*moduleHandles.Length),
                        out size,
                        NativeMethods.ModuleFilterFlags.All);
                }

                gcHandle.Free();

                if (x == 50)
                {
                    throw new Win32Exception();
                }

                size /= (uint)IntPtr.Size;

                if (size <= moduleHandles.Length)
                {
                    break;
                }

                moduleHandles = new IntPtr[moduleHandles.Length * 2];
            }

            if (moduleHandles.Length != size)
            {
                Array.Resize(ref moduleHandles, (int)size);
            }

            return moduleHandles;
        }

        private struct DataDirectory
        {
            public readonly uint Rva;

            public readonly uint Size;

            public DataDirectory(uint rva, uint size)
                : this()
            {
                Rva = rva;
                Size = size;
            }
        }
    }
}
