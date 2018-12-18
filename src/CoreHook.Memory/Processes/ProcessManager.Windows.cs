using System;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using Bleak;

namespace CoreHook.Memory.Processes
{
    public sealed class ProcessManager : IProcessManager
    {
        private readonly IMemoryManager _memoryManager;
        private readonly IProcess _process;

        private readonly Injector _injector;
        
        public ProcessManager(IProcess process)
        {
            _process = process;
            _memoryManager = new MemoryManager(process);
            
            // Initialise an injector
            
            _injector = new Injector();
        }
        
        public void InjectBinary(string modulePath)
        {
            // Ensure the module path is valid

            if (string.IsNullOrWhiteSpace(modulePath))
            {
                throw new ArgumentException("Module path was invalid");
            }
            
            // Inject the dll using the manual map method
            
            _injector.ManualMap(modulePath, _process.ProcessId);
        }

        /// <summary>
        /// Execute function inside the specified module with custom arguments.
        /// </summary>
        /// <param name="module">The name of the module containing the desired function.</param>
        /// <param name="function">The name of the exported function we will call.</param>
        /// <param name="arguments">Serialized arguments for passing to the module function.</param>
        /// <param name="waitForThreadExit">We can wait for the thread to exit and then deallocate any memory
        /// we allocated or return immediately and deallocate the memory in a separate call.</param>
        public IntPtr Execute(string module, string function, byte[] arguments, bool waitForThreadExit = true)
        {
            return ExecuteFunction(module, function, arguments, waitForThreadExit);
        }

        private IntPtr ExecuteFunction(string module, string function, byte[] arguments, bool waitForThreadExit = true)
        {            
            SafeWaitHandle remoteThread = null;

            var argumentsAllocation =
                _memoryManager.Allocate(
                    arguments.Length,
                    MemoryProtectionType.ReadWrite, !waitForThreadExit);

            if (argumentsAllocation.Address == IntPtr.Zero)
            {
                throw new Win32Exception("Failed to allocate memory in remote process.");
            }

            try
            {
                var processHandle = _process.SafeHandle;
                // Write the arguments buffer to our allocated address
                _memoryManager.WriteMemory(argumentsAllocation.Address.ToInt64(), arguments);

                // Execute the function call in a new thread
                remoteThread = ThreadHelper.CreateRemoteThread(processHandle,
                    ThreadHelper.GetProcAddress(processHandle, module, function),
                    argumentsAllocation.Address);

                if (waitForThreadExit)
                {
                    Interop.Kernel32.WaitForSingleObject(
                        remoteThread,
                        System.Threading.Timeout.Infinite);
                }

                return argumentsAllocation.Address;
            }
            finally
            {
                remoteThread?.Dispose();
                if (waitForThreadExit)
                {
                    _memoryManager.Deallocate(argumentsAllocation);
                }
            }
        }

        public IntPtr CopyToProcess(byte[] data, int? size)
        {
            int dataLen = size ?? data.Length;
            IntPtr allocationAddress = _memoryManager.Allocate(dataLen, 
                MemoryProtectionType.ReadWrite).Address;

            _memoryManager.WriteMemory(allocationAddress.ToInt64(), data);
 
            return allocationAddress;
        }

        public void Dispose()
        {
            _memoryManager?.Dispose();
            GC.SuppressFinalize(this);
        }

        ~ProcessManager() => Dispose();
    }
}
