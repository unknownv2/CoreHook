using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace CoreHook.Memory.Processes
{
    public sealed partial class ProcessManager : IProcessManager
    {
        public Process ProcessHandle { get; private set; }
        public SafeProcessHandle SafeHandle { get; private set; }

        private readonly IMemoryManager _memoryManager;

        public ProcessManager(Process process, IMemoryManager memoryManager)
        {
            ProcessHandle = process;
            _memoryManager = memoryManager;
        }

        private SafeProcessHandle GetProcessHandle(int processId, int access)
        {
            SafeProcessHandle handle = Interop.Kernel32.OpenProcess(access, false, processId);

            SafeHandle = handle ?? throw new UnauthorizedAccessException("Failed to open process handle.");

            return handle;
        }

        public void InjectBinary(string modulePath)
        {
            ExecuteFuntion(Path.Combine(
                             Environment.ExpandEnvironmentVariables("%Windir%"),
                             "System32",
                             "kernel32.dll"),
                             "LoadLibraryW",
                             Encoding.Unicode.GetBytes(modulePath + "\0"));
        }

        /// <summary>
        /// Execute function inside the specified module with custom arguments.
        /// </summary>
        /// <param name="module">The name of the module containing the desired function.</param>
        /// <param name="function">The name of the exported function we will call.</param>
        /// <param name="arguments">Serialized arguments for passing to the module function.</param>
        /// <param name="canWait">We can wait for the thread to exit and then deallocate any memory
        /// we allocated or return immediately and deallocate the memory in a separate call.</param>
        public IntPtr Execute(string module, string function, byte[] arguments, bool canWait = true)
            => ExecuteFuntion(module, function, arguments, canWait);

        private IntPtr ExecuteFuntion(string module, string function, byte[] arguments, bool canWait = true)
        {
            SafeWaitHandle remoteThread = null;
            using (var processHandle = GetProcessHandle(ProcessHandle.Id,
                Interop.Advapi32.ProcessOptions.PROCESS_CREATE_THREAD |
                Interop.Advapi32.ProcessOptions.PROCESS_QUERY_INFORMATION |
                Interop.Advapi32.ProcessOptions.PROCESS_VM_OPERATION |
                Interop.Advapi32.ProcessOptions.PROCESS_VM_READ |
                Interop.Advapi32.ProcessOptions.PROCESS_VM_WRITE))
            {

                var argumentsAllocation =
                    _memoryManager.Allocate(
                        arguments.Length,
                        MemoryProtectionType.ReadWrite, !canWait);

                if (argumentsAllocation.Address == IntPtr.Zero)
                {
                    throw new Win32Exception("Failed to allocate memory in remote process.");
                }

                try
                {
                    // Write the arguments buffer to our allocated address
                    _memoryManager.WriteMemory(argumentsAllocation.Address.ToInt64(), arguments);

                    // Execute the function call in a new thread
                    remoteThread = ThreadHelper.CreateRemoteThread(processHandle,
                        ThreadHelper.GetProcAddress(processHandle, module, function), 
                        argumentsAllocation.Address);

                    if (canWait)
                    {
                        const int infiniteWait = -1;
                        Interop.Kernel32.WaitForSingleObject(
                            remoteThread,
                            infiniteWait);
                    }

                    return argumentsAllocation.Address;
                }
                finally
                {
                    remoteThread?.Dispose();
                    if (canWait)
                    {
                        _memoryManager.Deallocate(argumentsAllocation);
                    }
                }
            }
        }

        public IntPtr CopyToProcess(byte[] data, int? size)
        {
            int dataLen = size ?? data.Length;
            IntPtr remoteAllocAddr = _memoryManager.Allocate(dataLen, 
                MemoryProtectionType.ReadWrite).Address;

            _memoryManager.WriteMemory(remoteAllocAddr.ToInt64(), data);
 
            return remoteAllocAddr;
        }

        public void Dispose()
        {
            _memoryManager?.Dispose();
            GC.SuppressFinalize(this);
        }

        ~ProcessManager()
        {
            Dispose();
        }
    }
}
