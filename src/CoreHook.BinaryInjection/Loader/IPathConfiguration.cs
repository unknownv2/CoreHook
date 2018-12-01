
namespace CoreHook.BinaryInjection.Loader
{
    public interface IPathConfiguration : IStringEncodingConfiguration
    {
        int MaxPathLength { get; }
    }
}
