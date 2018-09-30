using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CoreHook.Unmanaged;

namespace CoreHook.BinaryInjection
{
    public class WindowsBinaryLoader : IBinaryLoader
    {
        /// <summary>
        /// The name of the function that starts the CoreCLR in a target process
        /// and can also execute a .NET assembly immediately.
        /// </summary>
        private const string LoadAssemblyFunc = "LoadAssembly";

        /// <summary>
        /// The name of a function that executes a single function inside a .NET library loaded in a process,
        /// referenced by class name and function name.
        /// </summary>
        private const string ExecAssemblyFunc = "ExecuteAssemblyFunction";


        private readonly IMemoryManager _memoryManager;

        public WindowsBinaryLoader(IMemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
            _memoryManager.FreeMemory += FreeMemory;
        }

        private void ExecuteAssemblyWithArgs(Process process, string module, WindowsBinaryLoaderArgs args)
        {
            _memoryManager.Add(
                process,
                process.Execute(module, LoadAssemblyFunc, Binary.StructToByteArray(args)),
                true
            );
        }

        /// <summary>
        ///  Execute a function in a process in a new thread with a <see cref="FunctionCallArgs" /> argument
        /// </summary>
        /// <param name="process">The process the thread will be created and executed in.</param>
        /// <param name="moduleName">The module name of the binary containing the function to execute.</param>
        /// <param name="functionName">The name of the function to be executed.</param>
        /// <param name="args">The class which will be serialized and passed to the function being executed.</param>
        private void ExecuteAssemblyFunctionWithArgs(Process process, string moduleName, string functionName, FunctionCallArgs args)
        {
            _memoryManager.Add(
                process,
                process.Execute(moduleName, functionName, Binary.StructToByteArray(args), false),
                false
            );
        }

        public void ExecuteWithArgs(Process process, string module, BinaryLoaderArgs args)
        {
            ExecuteAssemblyWithArgs(process, module, WindowsBinaryLoaderArgs.Create(args));
        }

        public void CallFunctionWithArgs(Process process, string module, string function, byte[] arguments)
        {
            ExecuteAssemblyFunctionWithArgs(process, module, ExecAssemblyFunc, new FunctionCallArgs(function, arguments));
        }

        public void CallFunctionWithRemoteArgs(Process process, string module, string function, BinaryLoaderArgs blArgs, RemoteFunctionArgs rfArgs)
        {
            ExecuteWithArgs(process, module, blArgs);
            ExecuteAssemblyFunctionWithArgs(process, module, ExecAssemblyFunc, new FunctionCallArgs(function, rfArgs));
        }

        public void CallFunctionWithRemoteArgs(Process process, string module, string function, IntPtr arguments)
        {
            ExecuteAssemblyFunctionWithArgs(process, module, ExecAssemblyFunc, new FunctionCallArgs(function, arguments));
        }

        public IntPtr CopyMemoryTo(Process proc, byte[] buffer, uint length)
        {
            return _memoryManager.Add(
                proc,
                proc.MemCopyTo(buffer, length),
                false
            );
        }

        public static bool FreeMemory(Process proc, IntPtr address, uint length = 0)
        {
            return proc.FreeMemory(address, 0);
        }

        public void Load(Process targetProcess, string binaryPath, IEnumerable<string> dependencies = null, string dir = null)
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

                    targetProcess.LoadLibrary(binary);
                }
            }

            targetProcess.LoadLibrary(binaryPath);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _memoryManager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.               

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WindowsBinaryLoader() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
