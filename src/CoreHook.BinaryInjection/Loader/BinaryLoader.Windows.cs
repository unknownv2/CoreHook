using System;
using System.Collections.Generic;
using System.IO;
using CoreHook.BinaryInjection.Loader.Serializer;
using CoreHook.BinaryInjection.Host;
using CoreHook.Memory;

namespace CoreHook.BinaryInjection.Loader
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
        /// <param name="functionName">The name of the function to be executed.</param>
        /// <param name="arguments">The class which will be serialized and passed to the function being executed.</param>
        private void ExecuteAssemblyFunctionWithArguments(IFunctionName functionName, FunctionCallArguments arguments)
        {
            _processManager.Execute(functionName.Module, functionName.Function,
                MarshallingHelper.StructToByteArray(arguments), false);
        }

        private void ExecuteAssemblyWithArguments(IFunctionName moduleFunction, byte[] arguments) => 
            _processManager.Execute(moduleFunction.Module, moduleFunction.Function, arguments);

        public void ExecuteWithArguments(IFunctionName function, IBinarySerializer arguments) => 
            ExecuteAssemblyWithArguments(function, arguments.Serialize());

        public void ExecuteRemoteFunction(IRemoteFunctionCall call) => 
            ExecuteWithArguments(call.FunctionName, call.Arguments);

        public void ExecuteRemoteManagedFunction(IRemoteManagedFunctionCall call) => 
            ExecuteAssemblyFunctionWithArguments(
                call.FunctionName,
                new FunctionCallArguments(call.ManagedFunctionDelegate, call.Arguments));

        public IntPtr CopyMemoryTo(byte[] buffer, int length) => 
            _processManager.CopyToProcess(buffer, length);

        public void Load(
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
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _processManager.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BinaryLoader()
        {
            Dispose(false);
        }
        #endregion
    }
}
