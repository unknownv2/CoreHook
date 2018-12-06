
namespace CoreHook.BinaryInjection.Loader
{
    public interface IRemoteThreadCreator
    {
        void ExecuteRemoteFunction(IRemoteFunctionCall call, bool waitForThreadExit);
    }
}
