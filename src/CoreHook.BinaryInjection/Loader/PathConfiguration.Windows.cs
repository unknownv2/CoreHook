using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    public sealed partial class PathConfiguration : IPathConfiguration
    {
        public int MaxPathLength => 260;
        public Encoding PathEncoding => Encoding.Unicode;
        public char PaddingCharacter => '\0';
    }
}
