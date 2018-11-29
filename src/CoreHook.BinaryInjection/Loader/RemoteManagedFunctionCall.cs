
namespace CoreHook.BinaryInjection.Loader
{
    public class RemoteManagedFunctionCall : RemoteFunctionCall, IRemoteManagedFunctionCall
    {
        public IAssemblyDelegate ManagedFunctionDelegate { get; set; }
    }
}
