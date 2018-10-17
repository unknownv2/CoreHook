namespace CoreHook.DependencyModel
{
    internal class FileSystemWrapper : IFileSystem
    {
        public static IFileSystem Default { get; } = new FileSystemWrapper();

        public IFile File { get; } = new FileWrapper();

        public IDirectory Directory { get; } = new DirectoryWrapper();
    }
}
