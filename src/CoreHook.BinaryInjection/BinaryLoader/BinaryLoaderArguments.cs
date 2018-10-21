using System.Text;

namespace CoreHook.BinaryInjection
{
    public class BinaryLoaderArguments
    {
        public bool Verbose;

        public bool WaitForDebugger;

        public string PayloadFileName;

        public string CoreRootPath;

        public string CoreLibrariesPath;

        public static byte[] GetPathArray(string path, int pathLength, Encoding encoding)
        {
            return encoding.GetBytes(path.PadRight(pathLength, '\0'));
        }
    }
}