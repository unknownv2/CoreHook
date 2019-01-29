using CoreHook.Memory.Processes;

namespace CoreHook.BinaryInjection.Loader
{
    internal class RemoteThreadCreator : IRemoteThreadCreator
    {
        private readonly IThreadManager _threadManager;

        internal RemoteThreadCreator(IThreadManager threadManager)
        {
            _threadManager = threadManager;
        }

        public void ExecuteRemoteFunction(IRemoteFunctionCall call, bool waitForThreadExit)
        {
            ExecuteAssemblyWithArguments(call.FunctionName, call.Arguments.Serialize(), waitForThreadExit);
        }

        private void ExecuteAssemblyWithArguments(IFunctionName moduleFunction, byte[] arguments, bool waitForThreadExit)
        {
            _threadManager.CreateThread(moduleFunction.Module, moduleFunction.Function, arguments, waitForThreadExit);
        }
    }
}
