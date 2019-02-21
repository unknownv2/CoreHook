using System.Text;

namespace CoreHook.BinaryInjection.Loader.Configuration
{
    public sealed partial class PathConfiguration : IPathConfiguration
    {
        public int MaxPathLength => 260;
        public Encoding Encoding => Encoding.Unicode;
        public char PaddingCharacter => '\0';
    }
}
