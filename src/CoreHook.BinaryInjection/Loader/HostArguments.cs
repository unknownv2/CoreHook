
namespace CoreHook.BinaryInjection.Loader
{
    public class HostArguments : IHostArguments
    {
        public bool Verbose { get; set; }
        public string PayloadFileName { get; set; }
        public string CoreRootPath { get; set; }
        public string CoreLibrariesPath { get; set; }
    }
}