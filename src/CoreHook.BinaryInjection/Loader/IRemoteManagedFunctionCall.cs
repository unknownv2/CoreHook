
namespace CoreHook.BinaryInjection.Loader
{
    public interface IRemoteManagedFunctionCall : IRemoteFunctionCall
    {
        IAssemblyDelegate ManagedFunctionDelegate { get; }
    }
}
