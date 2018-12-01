
namespace CoreHook.BinaryInjection.Loader
{
    public interface IHostArguments
    {
        bool Verbose { get; }
        string PayloadFileName { get; }
        string CoreRootPath { get; }
        string CoreLibrariesPath { get; }
    }
}
