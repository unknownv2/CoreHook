using CoreHook.BinaryInjection.Loader.Serialization;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IRemoteFunctionCall
    {
        IFunctionName FunctionName { get; }
        ISerializableObject Arguments { get; }
    }
}
