﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using CoreHook.Unmanaged;
using System.IO;
using System.Threading;

namespace CoreHook.BinaryInjection
{
    public class BinaryLoader : IBinaryLoader
    {
        private IMemoryManager _memoryManager;

        public BinaryLoader(IMemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
            _memoryManager.FreeMemory += FreeMemory;
        }
        // Inject an assembly into a process
        private string LoadAssemblyFunc = "LoadAssembly";
        private void ExecuteAssemblyWithArgs(Process process, string module, WindowsBinaryLoaderArgs args)
        {
            _memoryManager.Add(
                process,
                process.Execute(module, LoadAssemblyFunc, Binary.StructToByteArray(args)),
                true
            );
        }

        // Execute a function inside a library using the class name and function name
        private string ExecAssemblyFunc = "ExecuteAssemblyFunction";
        private void LoadAssemblyWithArgs(Process process, string module, FunctionCallArgs args)
        {
            _memoryManager.Add(
                process,
                process.Execute(module, ExecAssemblyFunc, Binary.StructToByteArray(args), false),
                false
            );
        }

        public void ExecuteWithArgs(Process process, string module, BinaryLoaderArgs args)
        {
            ExecuteAssemblyWithArgs(process, module, WindowsBinaryLoaderArgs.Create(args));
        }

        public void CallFunctionWithArgs(Process process, string module, string function, byte[] arguments)
        {
            LoadAssemblyWithArgs(process, module, new FunctionCallArgs(function, arguments));
        }
        public void CallFunctionWithRemoteArgs(Process process, string module, string function, BinaryLoaderArgs blArgs, RemoteFunctionArgs rfArgs)
        {
            ExecuteWithArgs(process, module, blArgs);
            LoadAssemblyWithArgs(process, module, new FunctionCallArgs(function, rfArgs));
        }
        public void CallFunctionWithRemoteArgs(Process process, string module, string function, IntPtr arguments)
        {
            LoadAssemblyWithArgs(process, module, new FunctionCallArgs(function, arguments));
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

                    //if (targetProcess.GetModuleHandleByBaseName(moduleName) == IntPtr.Zero)
                    //{
                    //    targetProcess.LoadLibrary(binary);
                    //  }
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
        // ~BinaryLoader() {
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