
namespace CoreHook.BinaryInjection.Loader
{
    internal static class PathArgumentsHelper
    {
        internal static byte[] GetPathArray(string path, IPathConfiguration pathConfig, int? pathLength = null)
        {
            return pathConfig.Encoding.GetBytes(path.PadRight(pathLength ?? pathConfig.MaxPathLength, pathConfig.PaddingCharacter));
        }
    }
}
