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
    public class LinuxBinaryLoader : IBinaryLoader
    {
        private int _processAttached = -1;

        private IntPtr _symbolHandle;

        private const int _symbolHandlerSize = 0x70;

        private Dictionary<string, IntPtr> _cachedFunctions = new Dictionary<string, IntPtr>();

        private const string _libcName = "libc";
        private const string _mallocName = "malloc";

        private const string _mailboxName = "RemoteThreadMailbox";

        private const string LinuxExecDotnetAssembly = "ExecuteDotnetAssembly";
        private const string LinuxExecAssemblyFunction = "ExecuteManagedAssemblyClassFunction";
        private const string LinuxLoadAssembly = "LoadAssemblyBinaryArgs";

        private long _mailboxAddress;

        private IntPtr _mailboxPtr { get { return new IntPtr(_mailboxAddress); } }

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
                _mailboxAddress = ReadIntPtr(pid, addr.ToInt64());
            }
            else
            {
                throw new Exception("Failed to find mailbox address");
            }
        }
        private static long PtraceReadPtr(int pid, long address)
        {
            var addrSize = IntPtr.Size;
            var addrPtr = Marshal.AllocHGlobal(addrSize);
            Unmanaged.Linux.Process.ptrace_read(pid, new IntPtr(address), addrPtr, addrSize);

            byte[] managedArray = new byte[addrSize];
            Marshal.Copy(addrPtr, managedArray, 0, addrSize);
            Marshal.FreeHGlobal(addrPtr);
            return (addrSize == 8 ? BitConverter.ToInt64(managedArray, 0)
                : BitConverter.ToUInt32(managedArray, 0));
        }
        private static void PtraceWrite(int pid, long addr, byte[] buffer, int length)
        {
            var addrSize = IntPtr.Size;
            var addrPtr = new IntPtr(addr);
            // clear memory
            Unmanaged.Linux.Process.ptrace_write(pid, addrPtr, new byte[length], length);
            // write new function args          
            Unmanaged.Linux.Process.ptrace_write(pid, addrPtr, buffer, length);
        }
        private static long ReadIntPtr(int pid, long address)
        {
            var addrSize = IntPtr.Size;
            var addrPtr = Marshal.AllocHGlobal(addrSize);       

            return PtraceReadPtr(pid, address);
        }
        private void PtraceAttach(int pid)
        {
            try
            {
                Unmanaged.Linux.Process.ptrace_attach(pid);

                _processAttached = pid;

                _symbolHandle = Marshal.AllocHGlobal(_symbolHandlerSize);

                Unmanaged.Linux.Process.set_pid(_symbolHandle, pid);
                Unmanaged.Linux.Process.parse_elf(_symbolHandle);
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
                var addr = Unmanaged.Linux.Process.find_symbol(_symbolHandle, function, libName == _libcName ? null : libName);
                if (addr != new IntPtr(-1))
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

        public void CallFunctionWithRemoteArgs(Process process, string module, string function, IntPtr arguments)
        {
            if (IsAttached(process.Id))
            {
                var pid = process.Id;
                var args = new LinuxFunctionCallArgs(function, arguments);
                var argsBuf = Binary.StructToByteArray(args);
                var bufPtr = CopyMemoryTo(process, argsBuf, (uint)argsBuf.Length);

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
                      StartAddress = GetCachedFunction(module, LinuxExecAssemblyFunction),
                      Params = bufPtr
                  },
                  false
                );
            }
        }


        public void CallFunctionWithRemoteArgs(Process process, string module, string function, RemoteFunctionArgs arguments)
        {
            if (IsAttached(process.Id))
            {
                var pid = process.Id;
                var args = new LinuxFunctionCallArgs(function, arguments);
                var argsBuf = Binary.StructToByteArray(args);
                var bufPtr = CopyMemoryTo(process, argsBuf, (uint)argsBuf.Length);

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
                      StartAddress = GetCachedFunction(module, LinuxExecAssemblyFunction),
                      Params = bufPtr
                  },
                  false
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
                  Params = new IntPtr(length)
              }
            );

            var memAddrLong = ReadIntPtr(id, _mailboxAddress + 8);

            WriteBuffer(id, memAddrLong, buffer, (int)length);

            return new IntPtr(memAddrLong);
        }
        private static void SendRpcRequest(int pid, long mailbox, object args, bool waitForRet = true)
        {
            var rpcArgs = Binary.StructToByteArray(args);

            PtraceWrite(pid, mailbox, rpcArgs, rpcArgs.Length);
            if (waitForRet)
            {
                WaitForRpc(pid, mailbox);
            }
        }
        private static void WaitForRpc(int pid, long mailbox, int sleepTime = 100)
        {
            var statusSize = 4;
            var statusPtr = Marshal.AllocHGlobal(statusSize);
            byte[] statusArray = new byte[statusSize];

            bool IsRpcComplete()
            {
                Unmanaged.Linux.Process.ptrace_read(pid, new IntPtr(mailbox), statusPtr, statusSize);

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
            WriteBuffer(pid, mailbox, msgArgs, msgArgs.Length);
        }
        private static void WriteBuffer(int pid, long addr, byte[] buffer, int length)
        {
            PtraceWrite(pid, addr, buffer, length);
        }
        public void Execute(Process process, string module, string function, string args)
        {
            if (IsAttached(process.Id))
            {
                var addr = GetFunctionAddress(module, function);
            }
        }
        public void ExecuteWithArgs(Process process, string module, object args)
        {
            if (IsAttached(process.Id))
            {
                var pid = process.Id;
                var argsBuf = Binary.StructToByteArray(args);
                var bufPtr = CopyMemoryTo(process, argsBuf, (uint)argsBuf.Length);
 
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
                       Params = bufPtr
                   }
                );
            }
        }

        public void Load(Process targetProcess, string binaryPath, IEnumerable<string> dependencies = null, string dir = null)
        {
            var injHandle = StartInjection(targetProcess.Id);
            if (dependencies != null)
            {
                foreach (var binary in dependencies)
                {
                    var libName = Path.Combine(dir, binary);
                    if (!File.Exists(libName))
                    {
                        throw new FileNotFoundException("Binary file not found.", binary);
                    }
                    InjectLibrary(injHandle, libName);
                }
            }

            InjectLibrary(injHandle, binaryPath);

            EndInjection(injHandle);

            PtraceAttach(targetProcess.Id);

            GetMailboxAddress(targetProcess.Id, binaryPath);
        }

        private IntPtr StartInjection(int pid)
        {
            var addrSize = IntPtr.Size;
            var addrPtr = Marshal.AllocHGlobal(addrSize);
            Unmanaged.Linux.ProcessLibInjector.injector_attach(ref addrPtr, pid);
            return addrPtr;
        }
        private int InjectLibrary(IntPtr handle, string libraryName)
        {
            return Unmanaged.Linux.ProcessLibInjector.injector_inject(handle, libraryName);
        }
        private int EndInjection(IntPtr handle)
        {
            // injector_detach frees our injector handle so no need to do it ourselves
            return Unmanaged.Linux.ProcessLibInjector.injector_detach(handle);
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
