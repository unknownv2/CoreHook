using CoreHook.BinaryInjection.Loader.Serializer;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IRemoteFunctionCall
    {
        IFunctionName FunctionName { get; }
        IBinarySerializer Arguments { get; }
    }
}
