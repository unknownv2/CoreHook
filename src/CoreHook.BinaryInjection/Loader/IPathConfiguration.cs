using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IPathConfiguration : IPathEncodingConfiguration
    {
        int MaxPathLength { get; }
    }
}
