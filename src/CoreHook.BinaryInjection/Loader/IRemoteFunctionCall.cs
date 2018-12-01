using CoreHook.BinaryInjection.Loader.Serializers;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IRemoteFunctionCall
    {
        IFunctionName FunctionName { get; }
        IBinarySerializer Arguments { get; }
    }
}
