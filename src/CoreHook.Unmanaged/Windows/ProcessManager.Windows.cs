using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace CoreHook.Unmanaged.Windows
{
    public class ProcessManager : IProcessManager
    {
        public Process ProcessHandle { get; private set; }

        public ProcessManager() { }

        public ProcessManager(Process process)
        {
            ProcessHandle = process;
        }

        public void OpenHandle(Process process)
        {
            ProcessHandle?.Dispose();
            ProcessHandle = process;
        }

        private static SafeProcessHandle GetProcessHandle(int processId, NativeMethods.ProcessAccessFlags accessFlags)
        {
            SafeProcessHandle handle = NativeMethods.OpenProcess(accessFlags, false, processId);

            if (handle == null)
            {
                throw new UnauthorizedAccessException("Failed to open process with query access.");
            }

            return handle;
        }

        private static SafeProcessHandle GetReadProcessHandle(int processId)
        {
            return GetProcessHandle(processId,
                NativeMethods.ProcessAccessFlags.QueryInformation
                | NativeMethods.ProcessAccessFlags.VirtualMemoryRead);
        }

        public bool BringToFront()
        {
            if (IsActiveWindow())
            {
                return true;
            }

            var hWindow = ProcessHandle.MainWindowHandle;

            var style = NativeMethods.GetWindowLong(hWindow, NativeMethods.GWL_STYLE);

            if ((style & NativeMethods.WS_MINIMIZE) != 0)
            {
                NativeMethods.ShowWindow(hWindow, NativeMethods.ShowWindowCommand.Normal);
            }

            return NativeMethods.BringWindowToTop(ProcessHandle.MainWindowHandle);
        }

        public bool IsActiveWindow()
        {
            if (ProcessHandle.HasExited)
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

            return Marshal.GetLastWin32Error() == 0 && pid == ProcessHandle.Id;
        }

        public IntPtr GetWin32ProcAddress(string module, string function)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                if (ProcessHandle.Is64Bit())
                {
                    if (!Environment.Is64BitProcess)
                    {
                        throw new InvalidOperationException("Cannot get 64-bit proc address from a 32-bit process.");
                    }
                }
            }

            return GetAbsoluteFunctionAddressEx(module, function);
        }

        public void InjectBinary(string modulePath)
        {
            using (var hProcess = GetProcessHandle(ProcessHandle.Id,
                NativeMethods.ProcessAccessFlags.CreateThread |
                NativeMethods.ProcessAccessFlags.QueryInformation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryOperation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryRead |
                NativeMethods.ProcessAccessFlags.VirtualMemoryWrite))
            {
                var pathBytes = Encoding.Unicode.GetBytes(modulePath + "\0");

                // Allocate space in the remote process for the DLL path. 
                var remoteAllocAddr = NativeMethods.VirtualAllocEx(
                    hProcess,
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
                        hProcess,
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
                     hProcess,
                     IntPtr.Zero,
                     0,
                     GetWin32ProcAddress(
                         Path.Combine(
                             Environment.ExpandEnvironmentVariables("%Windir%"),
                             "System32",
                             "kernel32.dll"
                             ), "LoadLibraryW"),
                         remoteAllocAddr,
                         0,
                         IntPtr.Zero);

                    if (hThread == IntPtr.Zero)
                    {
                        throw new Win32Exception("Failed to create thread in remote process.");
                    }

                    NativeMethods.WaitForSingleObject(hThread, NativeMethods.INFINITE);

                    NativeMethods.CloseHandle(hThread);
                }
                finally
                {
                    NativeMethods.VirtualFreeEx(hProcess, remoteAllocAddr, 0, NativeMethods.FreeType.Release);
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
        public IntPtr Execute(string module, string function, byte[] args, bool canWait = true)
        {
            using (var hProcess = GetProcessHandle(ProcessHandle.Id,
                NativeMethods.ProcessAccessFlags.CreateThread |
                NativeMethods.ProcessAccessFlags.QueryInformation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryOperation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryRead |
                NativeMethods.ProcessAccessFlags.VirtualMemoryWrite))
            {

                // Allocate space in the remote process for the DLL path. 
                IntPtr remoteAllocAddr = NativeMethods.VirtualAllocEx(
                    hProcess,
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
                    bool result = NativeMethods.WriteProcessMemory(
                        hProcess,
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
                    IntPtr hThread = NativeMethods.CreateRemoteThread(
                        hProcess,
                        IntPtr.Zero,
                        0,
                        GetAbsoluteFunctionAddressEx(module, function),
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
                        NativeMethods.VirtualFreeEx(hProcess, remoteAllocAddr, 0, NativeMethods.FreeType.Release);
                    }
                }
            }
        }

        public IntPtr MemAllocate(uint size)
        {
            using (var hProcess = GetProcessHandle(ProcessHandle.Id,
                NativeMethods.ProcessAccessFlags.QueryInformation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryOperation |
                NativeMethods.ProcessAccessFlags.VirtualMemoryRead |
                NativeMethods.ProcessAccessFlags.VirtualMemoryWrite))
            {
                // Allocate space in the remote process for the DLL path. 
                IntPtr remoteAllocAddr = NativeMethods.VirtualAllocEx(
                    hProcess,
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

        public IntPtr MemCopyTo(byte[] data, uint size = 0)
        {
            using (var hProcess = GetProcessHandle(ProcessHandle.Id,
                  NativeMethods.ProcessAccessFlags.QueryInformation |
                  NativeMethods.ProcessAccessFlags.VirtualMemoryOperation |
                  NativeMethods.ProcessAccessFlags.VirtualMemoryRead |
                  NativeMethods.ProcessAccessFlags.VirtualMemoryWrite))
            {
                uint dataLen = size > 0 ? size : (uint)data.Length;
                IntPtr remoteAllocAddr = MemAllocate(dataLen);
                UIntPtr bytesWritten;

                // Write the DLL path to the allocated memory.
                bool result = NativeMethods.WriteProcessMemory(
                    hProcess,
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

        public bool FreeMemory(IntPtr address, int size = 0)
        {
            if (address == IntPtr.Zero)
                return true;

            using (var hProcess = GetProcessHandle(ProcessHandle.Id,
                  NativeMethods.ProcessAccessFlags.QueryInformation |
                  NativeMethods.ProcessAccessFlags.VirtualMemoryOperation))
            {
                return size == 0 ?
                    NativeMethods.VirtualFreeEx(hProcess, address, 0, NativeMethods.FreeType.Release)
                    : NativeMethods.VirtualFreeEx(hProcess, address, size, NativeMethods.FreeType.Decommit);
            }
        }
        public bool Is64Bit()
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                return false;
            }

            SafeProcessHandle handle = NativeMethods.OpenProcess(
                NativeMethods.ProcessAccessFlags.QueryInformation,
                false,
                ProcessHandle.Id
            );

            if (handle == null)
            {
                throw new Win32Exception();
            }

            using (handle)
            {
                bool ret;

                if (!NativeMethods.IsWow64Process(handle, out ret))
                {
                    throw new Win32Exception();
                }

                return !ret;
            }
        }

        public IntPtr GetAbsoluteFunctionAddressEx(string moduleFileName, string functionName)
        {
            var hProcess = GetReadProcessHandle(ProcessHandle.Id);

            IntPtr hModule = GetModuleHandleByFileName(hProcess, moduleFileName);

            if (hModule == IntPtr.Zero)
            {
                throw new ModuleNotFoundException("Module not found in process.");
            }

            return GetAbsoluteFunctionAddress(hProcess, hModule, functionName);
        }

        private static IntPtr GetAbsoluteFunctionAddress(SafeProcessHandle hProcess, IntPtr hModule, string functionName)
        {
            var moduleInfo = GetModuleInfo(hProcess, hModule);

            DataDirectory exportDir = GetDataDirectory(ReadPage(hProcess, moduleInfo.BaseAddress), 0);

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

        private static NativeMethods.MODULEINFO GetModuleInfo(SafeProcessHandle hProcess, IntPtr hModule)
        {
            NativeMethods.MODULEINFO moduleInfo;

            if (!NativeMethods.GetModuleInformation(hProcess, hModule, out moduleInfo, (uint)Marshal.SizeOf<NativeMethods.MODULEINFO>()))
            {
                throw new Win32Exception("Failed to get module information.");
            }

            return moduleInfo;
        }

        private static byte[] ReadPage(SafeProcessHandle hProcess, IntPtr address)
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

                    default:
                        throw new BadImageFormatException("Unknown optional header magic in PE header.");
                }

                io.BaseStream.Position += 8 * index;

                var rva = io.ReadUInt32();

                var dir = new DataDirectory(rva, io.ReadUInt32());

                io.Close();

                return dir;
            }
        }

        private static IntPtr GetFunctionAddress(byte[] exportTable, uint exportTableRva, string functionName)
        {
            var ms = new MemoryStream(exportTable);

            using (var io = new BinaryReader(ms))
            {
                // Skip flags, timestamp, version, and name of DLL RVA.
                ms.Position = 16;

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

        public static IntPtr GetModuleHandleByBaseName(SafeProcessHandle hProcess, string moduleName)
        {
            IntPtr[] handles = GetAllModuleHandles(hProcess);

            foreach (IntPtr moduleHandle in handles)
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

        public static IntPtr GetModuleHandleByFileName(SafeProcessHandle hProcess, string moduleName)
        {
            IntPtr[] handles = GetAllModuleHandles(hProcess);

            foreach (IntPtr moduleHandle in handles)
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

        private static IntPtr[] GetAllModuleHandles(SafeProcessHandle hProcess)
        {
            var moduleHandles = new IntPtr[64];

            uint size = 0;

            for (; ; )
            {
                var gcHandle = GCHandle.Alloc(moduleHandles, GCHandleType.Pinned);
                
               if(!NativeMethods.EnumProcessModulesEx(
                    hProcess,
                    gcHandle.AddrOfPinnedObject(),
                    (uint)(IntPtr.Size * moduleHandles.Length),
                    out size,
                    NativeMethods.ModuleFilterFlags.All))
                {
                    throw new Win32Exception("EnumProcessModulesEx failed");
                }                

                gcHandle.Free();

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
