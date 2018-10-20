using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CoreHook.BinaryInjection.BinaryLoader.Memory;
using CoreHook.BinaryInjection.Host;
using CoreHook.Unmanaged;

namespace CoreHook.BinaryInjection.BinaryLoader.Windows
{
    public partial class BinaryLoader : IBinaryLoader
    {
        private readonly IMemoryManager _memoryManager;
        private readonly IProcessManager _processManager;

        public BinaryLoader(IMemoryManager memoryManager, IProcessManager processManager)
        {
            _memoryManager = memoryManager;
            _processManager = processManager;
            _memoryManager.FreeMemory += (proc, address, length) => processManager.FreeMemory(address);
        }

        /// <summary>
        ///  Execute a function in a process in a new thread with a <see cref="FunctionCallArguments" /> argument
        /// </summary>
        /// <param name="process">The process the thread will be created and executed in.</param>
        /// <param name="moduleName">The module name of the binary containing the function to execute.</param>
        /// <param name="functionName">The name of the function to be executed.</param>
        /// <param name="arguments">The class which will be serialized and passed to the function being executed.</param>
        private void ExecuteAssemblyFunctionWithArguments(
            Process process,
            IFunctionName functionName,
            FunctionCallArguments arguments)
        {
            _memoryManager.Add(
                process,
                _processManager.Execute(
                    functionName.Module,
                    functionName.Function,
                    Binary.StructToByteArray(arguments),
                    false),
                false
            );
        }

        private void ExecuteAssemblyWithArguments(Process process, IFunctionName moduleFunction, byte[] arguments)
        {
            _memoryManager.Add(
                process,
                _processManager.Execute(moduleFunction.Module, moduleFunction.Function, arguments),
                true
            );
        }

        public void ExecuteWithArguments(Process process, IFunctionName function, IBinarySerializer arguments)
            => ExecuteAssemblyWithArguments(process, function, arguments.Serialize());

        public void ExecuteRemoteFunction(Process process, IRemoteFunctionCall call) 
            => ExecuteWithArguments(process, call.FunctionName, call.Arguments);

        public void ExecuteRemoteManagedFunction(Process process, IRemoteManagedFunctionCall call) 
            => ExecuteAssemblyFunctionWithArguments(
                process, 
                call.FunctionName, 
                new FunctionCallArguments(call.ManagedFunction, call.Arguments));

        public IntPtr CopyMemoryTo(Process process, byte[] buffer, int length) 
            => _memoryManager.Add(process, _processManager.MemCopyTo(buffer, length), false);

        public void Load(
            Process targetProcess,
            string binaryPath,
            IEnumerable<string> dependencies = null,
            string baseDirectory = null)
        {
            if (dependencies != null)
            {
                foreach (var binary in dependencies)
                {
                    if (!File.Exists(binary))
                    {
                        throw new FileNotFoundException("Binary file not found.", binary);
                    }

                    var moduleName = Path.GetFileName(binary);

                    _processManager.InjectBinary(binary);
                }
            }

            _processManager.InjectBinary(binaryPath);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _memoryManager.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
