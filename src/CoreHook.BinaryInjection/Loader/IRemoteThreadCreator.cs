
namespace CoreHook.BinaryInjection.Loader
{
    interface IRemoteThreadCreator
    {
        void ExecuteRemoteFunction(IRemoteFunctionCall call, bool waitForThreadExit);
    }
}
