using CoreHook.BinaryInjection.Loader.Serializer;

namespace CoreHook.BinaryInjection.Loader
{
    public class RemoteFunctionCall : IRemoteFunctionCall
    {
        public IFunctionName FunctionName { get; set; }
        public IBinarySerializer Arguments { get; set; }
    }
}
