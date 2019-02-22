using System;
using CoreHook.BinaryInjection.Loader.Configuration;

namespace CoreHook.BinaryInjection.Loader
{
    internal static class PathArgumentsHelper
    {
        internal static byte[] GetPathArray(string path, IPathConfiguration pathConfig)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length >= pathConfig.MaxPathLength)
            {
                throw new ArgumentException(nameof(path));
            }
            return pathConfig.Encoding.GetBytes(path.PadRight(pathConfig.MaxPathLength, pathConfig.PaddingCharacter));
        }
    }
}
