using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IPathEncodingConfiguration
    {
        Encoding PathEncoding { get; }
        char PaddingCharacter { get; }
    }
}
