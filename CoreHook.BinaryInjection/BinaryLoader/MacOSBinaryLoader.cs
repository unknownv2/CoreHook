using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CoreHook.Unmanaged;

namespace CoreHook.BinaryInjection
{
    public class MacOSBinaryLoader : IBinaryLoader
    {
        //private const string ExecDotnetAssemblyFuncName = "ExecuteDotnetAssembly";
        private const string LoadAssemblyBinaryArgsFuncName = "LoadAssemblyBinaryArgs";
        private const string ExecManagedAssemblyClassFunctionName = "ExecuteManagedAssemblyClassFunction";

        private object _binaryLoaderArgs;

        private IMemoryManager _memoryManager;

        public MacOSBinaryLoader(IMemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
        }

        public void CallFunctionWithRemoteArgs(Process process, string module, string function, IntPtr arguments)
        {
            // combinary functioncallargs and binaryloader args
            var paramArgs = new DotnetAssemblyFunctionCall()
            {
                coreRunLib = System.Text.Encoding.ASCII.GetBytes(_coreRunLib.PadRight(1024, '\0')),
                binaryLoaderFunctionName = System.Text.Encoding.ASCII.GetBytes(LoadAssemblyBinaryArgsFuncName.PadRight(256, '\0')),
                assemblyCallFunctionName = System.Text.Encoding.ASCII.GetBytes(ExecManagedAssemblyClassFunctionName.PadRight(256, '\0')),
                binaryLoaderArgs = (MacOSBinaryLoaderArgs)_binaryLoaderArgs,
                assemblyFunctionCall = new LinuxFunctionCallArgs(function, arguments)
            };
            byte[] parameters = Binary.StructToByteArray(paramArgs);
            Unmanaged.MacOS.Process.injectByPidWithArgs(process.Id, parameters, parameters.Length);
        }

        public void CallFunctionWithRemoteArgs(Process process, string module, string function, RemoteFunctionArgs arguments)
        {
            // combinary functioncallargs and binaryloader args
            var paramArgs = new DotnetAssemblyFunctionCall()
            {
                coreRunLib = System.Text.Encoding.ASCII.GetBytes(_coreRunLib.PadRight(1024, '\0')),
                binaryLoaderFunctionName = System.Text.Encoding.ASCII.GetBytes(LoadAssemblyBinaryArgsFuncName.PadRight(256, '\0')),
                assemblyCallFunctionName = System.Text.Encoding.ASCII.GetBytes(ExecManagedAssemblyClassFunctionName.PadRight(256, '\0')),
                binaryLoaderArgs = (MacOSBinaryLoaderArgs)_binaryLoaderArgs,
                assemblyFunctionCall = new LinuxFunctionCallArgs(function, arguments)
            };
            byte[] parameters = Binary.StructToByteArray(paramArgs);
            Unmanaged.MacOS.Process.injectByPidWithArgs(process.Id, parameters, parameters.Length);
        }

        public IntPtr CopyMemoryTo(Process proc, byte[] buffer, uint length)
        {
            return Unmanaged.MacOS.Process.copyMemToProcessByPid(proc.Id, buffer, length);
        }
        public void ExecuteWithArgs(Process process, string module, object args)
        {
            _binaryLoaderArgs = args;
        }

        private string _coreRunLib;
        public void Load(Process targetProcess, string binaryPath, IEnumerable<string> dependencies = null, string dir = null)
        {
            if (dependencies != null)
            {
                foreach (var binary in dependencies)
                {
                    var fname = Path.Combine(dir, binary);
                    if (!File.Exists(fname))
                    {
                        throw new FileNotFoundException("Binary file not found.", binary);
                    }
                    //Unmanaged.MacOS.Process.injectByPid(targetProcess.Id, fname);
                }
            }

            _coreRunLib = binaryPath;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

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

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LinuxBinaryLoader() {
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

        public static bool FreeMemory(Process proc, IntPtr address, uint length)
        {
            throw new NotImplementedException();
        }

        ~MacOSBinaryLoader()
        {
            Dispose();
        }

        #endregion
    }
}
