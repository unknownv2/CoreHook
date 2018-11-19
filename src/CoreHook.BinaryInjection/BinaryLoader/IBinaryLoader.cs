using System;
using System.Collections.Generic;
using System.Diagnostics;
using CoreHook.BinaryInjection.BinaryLoader.Serializer;

namespace CoreHook.BinaryInjection.BinaryLoader
{
    public interface IBinaryLoader : IDisposable
    {
        void Load(
            Process targetProcess,
            string binaryPath,
            IEnumerable<string> dependencies = null,
            string baseDirectory = null);
        void ExecuteRemoteFunction(
            Process process, 
            IRemoteFunctionCall call);
        void ExecuteRemoteManagedFunction(
            Process process, 
            IRemoteManagedFunctionCall call);
        IntPtr CopyMemoryTo(
            Process process,
            byte[] buffer,
            int length);
    }
    public interface IFunctionName
    {
        string Module { get; }
        string Function { get; }
    }
    public class FunctionName : IFunctionName
    {
        public string Module { get; set; }
        public string Function { get; set; }
    }
    public interface IRemoteFunctionCall
    {
        bool Is64BitProcess { get; }
        IFunctionName FunctionName { get; }
        IBinarySerializer Arguments { get; }
    }
    public interface IRemoteManagedFunctionCall : IRemoteFunctionCall
    {
        IAssemblyDelegate ManagedFunction { get; }
    }
    public class RemoteFunctionCall : IRemoteFunctionCall
    {
        public bool Is64BitProcess { get;}
        public IFunctionName FunctionName { get; set; }
        public IBinarySerializer Arguments { get; set; }
    }
    public class RemoteManagedFunctionCall: RemoteFunctionCall, IRemoteManagedFunctionCall
    {
        public IAssemblyDelegate ManagedFunction { get; set; }
    }
}