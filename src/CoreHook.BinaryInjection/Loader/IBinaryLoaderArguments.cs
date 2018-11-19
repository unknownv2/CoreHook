
namespace CoreHook.BinaryInjection.Loader
{
    public interface IBinaryLoaderArguments
    {
        bool Verbose { get; }
        string PayloadFileName { get; }
        string CoreRootPath { get; }
        string CoreLibrariesPath { get; }
    }
}
