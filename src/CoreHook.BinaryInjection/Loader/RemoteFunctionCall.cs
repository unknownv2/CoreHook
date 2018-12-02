using CoreHook.BinaryInjection.Loader.Serialization;

namespace CoreHook.BinaryInjection.Loader
{
    public class RemoteFunctionCall : IRemoteFunctionCall
    {
        public IFunctionName FunctionName { get; set; }
        public ISerializableObject Arguments { get; set; }
    }
}
