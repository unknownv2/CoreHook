using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    internal static class BinaryLoaderArgumentsHelper
    {
        public static byte[] GetPathArray(string path, int pathLength, Encoding encoding)
        {
            return encoding.GetBytes(path.PadRight(pathLength, paddingChar: '\0'));
        }
    }
}
