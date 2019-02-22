using System.Text;

namespace CoreHook.BinaryInjection.Loader.Configuration
{
    public interface IStringEncodingConfiguration
    {
        Encoding Encoding { get; }
        char PaddingCharacter { get; }
    }
}
