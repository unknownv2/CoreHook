using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoreHook.BinaryInjection
{
    public interface IBinaryLoader : IDisposable
    {
        void Load(
            Process targetProcess,
            string binaryPath,
            IEnumerable<string> dependencies = null,
            string dir = null);
        void ExecuteRemoteFunction(
            Process process, 
            IRemoteFunctionCall call);
        void ExecuteRemoteManagedFunction(
            Process process, 
            IRemoteManagedFunctionCall call);
        IntPtr CopyMemoryTo(
            Process proc,
            byte[] buffer,
            int length);
    }
    public interface IFunctionName
    {
        string Module { get; set; }
        string Function { get; set; }
    }
    public class FunctionName : IFunctionName
    {
        public string Module { get; set; }
        public string Function { get; set; }
    }
    public interface IRemoteFunctionCall
    {
        bool Is64BitProcess { get; set; }
        IFunctionName FunctionName { get; set; }
        IBinarySerializer Arguments { get; set; }
    }
    public interface IRemoteManagedFunctionCall : IRemoteFunctionCall
    {
        IAssemblyDelegate ManagedFunction { get; set; }
    }
    public class RemoteFunctionCall : IRemoteFunctionCall
    {
        public bool Is64BitProcess { get; set; }

        public IFunctionName FunctionName { get; set; }

        public IBinarySerializer Arguments { get; set; }
    }
    public class RemoteManagedFunctionCall: RemoteFunctionCall, IRemoteManagedFunctionCall
    {
        public IAssemblyDelegate ManagedFunction { get; set; }
    }
}