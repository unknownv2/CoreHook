using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IStringEncodingConfiguration
    {
        Encoding Encoding { get; }
        char PaddingCharacter { get; }
    }
}
