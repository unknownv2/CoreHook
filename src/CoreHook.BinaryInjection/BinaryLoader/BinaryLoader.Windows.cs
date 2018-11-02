using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CoreHook.BinaryInjection.Host;
using CoreHook.BinaryInjection.BinaryLoader.Serializer;
using CoreHook.Memory;

namespace CoreHook.BinaryInjection.BinaryLoader
{
    public partial class BinaryLoader : IBinaryLoader
    {
        private readonly IProcessManager _processManager;

        public BinaryLoader(IProcessManager processManager)
        {
            _processManager = processManager;
        }

        /// <summary>
        ///  Execute a function in a process in a new thread with a <see cref="FunctionCallArguments" /> argument
        /// </summary>
        /// <param name="process">The process the thread will be created and executed in.</param>
        /// <param name="functionName">The name of the function to be executed.</param>
        /// <param name="arguments">The class which will be serialized and passed to the function being executed.</param>
        private void ExecuteAssemblyFunctionWithArguments(
            Process process,
            IFunctionName functionName,
            FunctionCallArguments arguments)
        {
            _processManager.Execute(
                functionName.Module,
                functionName.Function,
                Binary.StructToByteArray(arguments),
                false);
        }

        private void ExecuteAssemblyWithArguments(Process process, IFunctionName moduleFunction, byte[] arguments)
        {
            _processManager.Execute(moduleFunction.Module, moduleFunction.Function, arguments);
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
            => _processManager.CopyToProcess(buffer, length);

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
                    _processManager.Dispose();
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
