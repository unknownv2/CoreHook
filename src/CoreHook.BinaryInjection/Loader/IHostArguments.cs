
namespace CoreHook.BinaryInjection.Loader
{
    /// <summary>
    /// Arguments passed to the native CLR hosting module that is
    /// injected into the target process and starts the runtime
    /// so we can execute .NET assembly functions in the remote process.
    /// </summary>
    public interface IHostArguments
    {
        bool Verbose { get; }
        string PayloadFileName { get; }
        string CoreRootPath { get; }
    }
}
