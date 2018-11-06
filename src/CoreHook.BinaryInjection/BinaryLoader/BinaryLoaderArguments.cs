
namespace CoreHook.BinaryInjection.BinaryLoader
{
    public class BinaryLoaderArguments : IBinaryLoaderArguments
    {
        public bool Verbose { get; set; }

        public bool WaitForDebugger { get; set; }

        public string PayloadFileName { get; set; }

        public string CoreRootPath { get; set; }

        public string CoreLibrariesPath { get; set; }
    }
}