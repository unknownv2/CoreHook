using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IPathConfiguration
    {
        int MaxPathLength { get; }
        Encoding PathEncoding { get; }
    }
}
