
namespace CoreHook.BinaryInjection
{
    public interface IAssemblyDelegate
    {
        string AssemblyName { get; }
        string TypeName { get; }
        string MethodName { get; }
    }
}
