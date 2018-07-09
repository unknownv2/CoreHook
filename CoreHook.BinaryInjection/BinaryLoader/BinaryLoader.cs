using System;
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
        private List<MemoryAllocation> _allocatedAddresses = new List<MemoryAllocation>();
        public void Execute(Process process, string module, string function, string args)
        {
            var argBytes = Encoding.Unicode.GetBytes(args + "\0");

            process.Execute(module, function, argBytes);
        }

        // Inject an assembly into a process
        private string LoadAssemblyFunc = "LoadAssembly";
        private void ExecuteAssemblyWithArgs(Process process, string module, BinaryLoaderArgs args)
        {
            _allocatedAddresses.Add(
                new MemoryAllocation()
                {
                    Process = process,
                    Address = process.Execute(module, LoadAssemblyFunc, Binary.StructToByteArray(args)),
                    IsFree = true
                });      
        }

        // Execute a function inside a library using the class name and function name
        private string ExecAssemblyFunc = "ExecuteAssemblyFunction";
        private void LoadAssemblyWithArgs(Process process, string module, FunctionCallArgs args)
        {
            _allocatedAddresses.Add(new MemoryAllocation()
            {
                Process = process,
                Address = process.Execute(module, ExecAssemblyFunc, Binary.StructToByteArray(args), false),
                IsFree = false
            });        
        }

        public void ExecuteWithArgs(Process process, string module, object args)
        {
            ExecuteAssemblyWithArgs(process, module, (BinaryLoaderArgs)args);
        }

        public void CallFunctionWithArgs(Process process, string module, string function, byte[] arguments)
        {
            LoadAssemblyWithArgs(process, module, new FunctionCallArgs(function, arguments));
        }
        public void CallFunctionWithRemoteArgs(Process process, string module, string function, RemoteFunctionArgs arguments)
        {
            LoadAssemblyWithArgs(process, module, new FunctionCallArgs(function, arguments));
        }
        public void CallFunctionWithRemoteArgs(Process process, string module, string function, IntPtr arguments)
        {
            LoadAssemblyWithArgs(process, module, new FunctionCallArgs(function, arguments));
        }
        public IntPtr CopyMemoryTo(Process proc, byte[] buffer, uint length)
        {
            return proc.MemCopyTo(buffer, length);
        }
        public bool FreeMemory(Process proc, IntPtr address, uint length = 0)
        {
            return proc.FreeMemory(address, (int)length);
        }
        public void Load(Process targetProcess, string binaryPath, IEnumerable<string> dependencies = null, string dir = null)
        {
            if (dependencies != null && dir != null)
            {
                foreach (var binary in dependencies)
                {
                    var fname = Path.Combine(dir, binary);
                    if (!File.Exists(fname))
                    {
                        throw new FileNotFoundException("Binary file not found.", binary);
                    }

                    var moduleName = Path.GetFileName(binary);

                    if (targetProcess.GetModuleHandleByBaseName(moduleName) == IntPtr.Zero)
                    {
                        targetProcess.LoadLibrary(fname);

                        var x = 0;
                        for (; x < 50 && targetProcess.GetModuleHandleByBaseName(moduleName) == IntPtr.Zero; x++)
                        {
                            Thread.Sleep(200);
                        }

                        if (x == 50)
                        {
                            throw new TimeoutException($"'{binary}' failed to load into the process.");
                        }
                    }
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
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                foreach(var memAlloc in _allocatedAddresses)
                {
                    if(memAlloc.Address != IntPtr.Zero && !memAlloc.IsFree)
                    {
                        if(!FreeMemory(memAlloc.Process, memAlloc.Address))
                        {
                            throw new Exception("Failed to free memory");
                        }
                    }
                }
                _allocatedAddresses.Clear();

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
    public class MemoryAllocation
    {
        public Process Process;
        public IntPtr Address;
        public bool IsFree;
    }
}
