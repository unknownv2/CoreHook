
namespace CoreHook.BinaryInjection.Loader.Configuration
{
    public interface IPathConfiguration : IStringEncodingConfiguration
    {
        int MaxPathLength { get; }
    }
}
