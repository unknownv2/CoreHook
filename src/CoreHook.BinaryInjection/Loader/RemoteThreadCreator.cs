using CoreHook.Memory;

namespace CoreHook.BinaryInjection.Loader
{
    internal class RemoteThreadCreator : IRemoteThreadCreator
    {
        private readonly IProcessManager _processManager;

        internal RemoteThreadCreator(IProcessManager processManager)
        {
            _processManager = processManager;
        }

        public void ExecuteRemoteFunction(IRemoteFunctionCall call, bool waitForThreadExit)
        {
            ExecuteAssemblyWithArguments(call.FunctionName, call.Arguments.Serialize(), waitForThreadExit);
        }

        private void ExecuteAssemblyWithArguments(IFunctionName moduleFunction, byte[] arguments, bool waitForThreadExit)
        {
            _processManager.Execute(moduleFunction.Module, moduleFunction.Function, arguments, waitForThreadExit);
        }
    }
}
