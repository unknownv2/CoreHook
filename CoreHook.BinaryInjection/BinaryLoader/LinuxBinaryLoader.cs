using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using CoreHook.BinaryInjection.Host;
using CoreHook.Unmanaged;
using System.Threading;

namespace CoreHook.BinaryInjection
{
    public class LinuxBinaryLoader : IBinaryLoader, IDisposable
    {
        private int _processAttached = -1;

        private IntPtr _symbolHandle;

        private const int _symbolHandlerSize = 0x70;

        private Dictionary<string, IntPtr> _cachedFunctions = new Dictionary<string, IntPtr>();

        private const string _libcName = "libc";

        private const string _mailboxName = "RemoteThreadMailbox";

        private const string _mallocName = "malloc";

        private long _mailboxAddress;

        private IntPtr _mailboxPtr {  get { return new IntPtr(_mailboxAddress); } }

        private bool IsAttached(int pid)
        {
            return _processAttached == pid;
        }

        private void ClearCache()
        {
            if (_cachedFunctions.Count > 0)
            {
                _cachedFunctions.Clear();
            }
        }
        private void GetMailboxAddress(int pid, string module)
        {
            var addr = GetCachedFunction(module, _mailboxName);

            if (addr != IntPtr.Zero)
            {
                _mailboxAddress = ReadInt64(pid, addr.ToInt64());
            }
            else
            {
                throw new Exception("Failed to find mailbox address");
            }
        }
        private static long ReadInt64(int pid, long address)
        {
            var addrSize = IntPtr.Size;
            var addrPtr = Marshal.AllocHGlobal(addrSize);

            Unmanaged.Linux.Process.ptrace_read(pid, address, addrPtr, addrSize);
            byte[] managedArray = new byte[addrSize];
            Marshal.Copy(addrPtr, managedArray, 0, addrSize);

            return BitConverter.ToInt64(managedArray, 0);
        }
        private void PtraceAttach(int pid)
        {
            try
            {
                Unmanaged.Linux.Process.ptrace_attach(pid);

                _processAttached = pid;

                _symbolHandle = Marshal.AllocHGlobal(_symbolHandlerSize);

                
            }
            catch
            {

            }
        }

        private void PtraceDetach(int pid)
        {
            try
            {
                Unmanaged.Linux.Process.ptrace_detach(pid);

            }
            catch
            {

            }
        }

        private static string FormatCacheKey(string libName, string function)
        {
            return $"{libName}!{function}";
        }
        private IntPtr GetCachedFunction(string libName, string function)
        {
            var keyName = FormatCacheKey(libName, function);

            if (_cachedFunctions.ContainsKey(keyName))
            {
                return _cachedFunctions[keyName];
            }
            else
            {
                var addr = Unmanaged.Linux.Process.find_symbol(_symbolHandle, function, libName);
                if(addr != new IntPtr(-1))
                {
                    _cachedFunctions.Add(keyName, addr);
                    return addr;
                }
            }
            return IntPtr.Zero;
        }
        private IntPtr GetCachedLibcFunction(string function)
        {
            return GetCachedFunction(_libcName, function);
        }
        private IntPtr GetFunctionAddress(string libName, string function)
        {
            return GetCachedFunction(libName, function);
        }
        private const string LinuxExecAssembly = "ExecuteDotnetAssembly";

        public void CallFunctionWithRemoteArgs(Process process, string module, string function, IntPtr arguments)
        {
            if (IsAttached(process.Id))
            {
                var pid = process.Id;
                var args = new FunctionCallArgs(function, arguments);
                WriteRpcMsgArgs(pid, _mailboxAddress, args);

                SendRpcRequest(
                  pid,
                  _mailboxAddress,
                  new RemoteThreadArgs
                  {
                      Status = 1,
                      ProcFlags = 0,
                      Result = 0,
                      ThreadAttributes = 0,
                      CreationFlags = 1,
                      StackSize = 0,
                      StartAddress = GetCachedFunction(module, LinuxExecAssembly),
                      Params = _mailboxPtr
                  }
                );
            }
        }

        public IntPtr CopyMemoryTo(Process proc, byte[] buffer, uint length)
        {
            var id = proc.Id;

            SendRpcRequest(
              id,
              _mailboxAddress,
              new RemoteThreadArgs
              {
                  Status = 1,
                  ProcFlags = 0,
                  Result = 0,
                  ThreadAttributes = 0,
                  CreationFlags = 1,
                  StackSize = 0,
                  StartAddress = GetCachedLibcFunction(_mallocName),
                  Params = new IntPtr(length + 1)
              }
            );
            return new IntPtr(ReadInt64(id, _mailboxAddress + 8));
        }
        private static void SendRpcRequest(int pid, long mailbox, object args)
        {
            var rpcArgs = Binary.StructToByteArray(args);

            Unmanaged.Linux.Process.ptrace_write(pid, mailbox, rpcArgs, rpcArgs.Length);

            WaitForRpc(pid, mailbox);
        }
        private static void WaitForRpc(int pid, long mailbox, int sleepTime = 100)
        {
            var statusSize = 4;
            var statusPtr = Marshal.AllocHGlobal(statusSize);
            byte[] statusArray = new byte[statusSize];

            bool IsRpcComplete()
            {
                Unmanaged.Linux.Process.ptrace_read(pid, mailbox, statusPtr, statusSize);

                Marshal.Copy(statusPtr, statusArray, 0, statusSize);
                return BitConverter.ToInt32(statusArray, 0) == 0;
            }

            while (!IsRpcComplete())
            {
                Thread.Sleep(sleepTime);
            }

            Marshal.FreeHGlobal(statusPtr);
        }
        private static void WriteRpcMsgArgs(int pid, long mailbox, object args)
        {
            var msgArgs = Binary.StructToByteArray(args);

            // clear memory
            Unmanaged.Linux.Process.ptrace_write(pid, mailbox, new byte[msgArgs.Length + 1], msgArgs.Length + 1);
            // write new function args          
            Unmanaged.Linux.Process.ptrace_write(pid, mailbox, msgArgs, msgArgs.Length);
        }
        public void Execute(Process process, string module, string function, string args)
        {
            if (IsAttached(process.Id))
            {
                var addr = GetFunctionAddress(module, function);
            }
        }

        private const string LinuxLoadAssembly = "LoadAssemblyBinaryArgs";
        public void ExecuteWithArgs(Process process, string module, BinaryLoaderArgs args)
        {
            if (IsAttached(process.Id))
            {
                var pid = process.Id;
                WriteRpcMsgArgs(pid, _mailboxAddress, args);

                SendRpcRequest(
                  pid,
                  _mailboxAddress,
                  new RemoteThreadArgs
                  {
                      Status = 1,
                      ProcFlags = 0,
                      Result = 0,
                      ThreadAttributes = 0,
                      CreationFlags = 1,
                      StackSize = 0,
                      StartAddress = GetCachedFunction(module, LinuxLoadAssembly),
                      Params = _mailboxPtr
                  }
                );
            }
        }

        public void Load(Process targetProcess, string binaryPath, IEnumerable<string> dependencies = null, string dir = null)
        {
            PtraceAttach(targetProcess.Id);
            GetMailboxAddress(targetProcess.Id, binaryPath);
            if (dependencies != null)
            {
                foreach (var binary in dependencies)
                {
                    var fname = Path.Combine(dir, binary);
                    if (!File.Exists(fname))
                    {
                        throw new FileNotFoundException("Binary file not found.", binary);
                    }
                    Unmanaged.Linux.Process.injectByPid(targetProcess.Id, fname);
                }
            }

            Unmanaged.Linux.Process.injectByPid(targetProcess.Id, binaryPath);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ClearCache();
                }

                if (IsAttached(_processAttached))
                {
                    PtraceDetach(_processAttached);
                }

                _processAttached = -1;


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

        ~LinuxBinaryLoader()
        {
            Dispose();
        }

        #endregion
    }
}
