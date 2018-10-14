using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CoreHook.Unmanaged;

namespace CoreHook.BinaryInjection
{
    public class WindowsBinaryLoader : IBinaryLoader
    {
        private readonly IMemoryManager _memoryManager;
        private readonly IProcessManager _processManager;

        public WindowsBinaryLoader(IMemoryManager memoryManager, IProcessManager processManager)
        {
            _memoryManager = memoryManager;
            _processManager = processManager;
            _memoryManager.FreeMemory += (proc, address, length) => processManager.FreeMemory(address, 0);
        }

        /// <summary>
        ///  Execute a function in a process in a new thread with a <see cref="FunctionCallArgs" /> argument
        /// </summary>
        /// <param name="process">The process the thread will be created and executed in.</param>
        /// <param name="moduleName">The module name of the binary containing the function to execute.</param>
        /// <param name="functionName">The name of the function to be executed.</param>
        /// <param name="args">The class which will be serialized and passed to the function being executed.</param>
        private void ExecuteAssemblyFunctionWithArgs(
            Process process,
            IFunctionName functionName,
            FunctionCallArgs args)
        {
            _memoryManager.Add(
                process,
                _processManager.Execute(
                    functionName.Module,
                    functionName.Function,
                    Binary.StructToByteArray(args),
                    false),
                false
            );
        }

        private void ExecuteAssemblyWithArgs(Process process, IFunctionName function, byte[] args)
        {
            _memoryManager.Add(
                process,
                _processManager.Execute(function.Module, function.Function, args),
                true
            );
        }

        public void ExecuteWithArgs(Process process, IFunctionName function, IBinarySerializer args)
            => ExecuteAssemblyWithArgs(process, function, args.Serialize());

        public void ExecuteRemoteFunction(Process process, IRemoteFunctionCall call) 
            => ExecuteWithArgs(process, call.FunctionName, call.Arguments);

        public void ExecuteRemoteManagedFunction(Process process, IRemoteManagedFunctionCall call) 
            => ExecuteAssemblyFunctionWithArgs(
                process, 
                call.FunctionName, 
                new FunctionCallArgs(call.ManagedFunction, call.Arguments));

        public IntPtr CopyMemoryTo(Process proc, byte[] buffer, uint length) 
            => _memoryManager.Add(proc, _processManager.MemCopyTo(buffer, length), false);

        public void Load(
            Process targetProcess,
            string binaryPath,
            IEnumerable<string> dependencies = null,
            string dir = null)
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
