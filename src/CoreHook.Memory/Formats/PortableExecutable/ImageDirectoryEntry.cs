
namespace CoreHook.Memory.Formats.PortableExecutable
{
    internal enum ImageDirectoryEntry
    {
        ImageDirectoryEntryExport = 0,
        ImageDirectoryEntryImport,
        ImageDirectoryEntryResource,
        ImageDirectoryEntryException,
        ImageDirectoryEntrySecurity,
        ImageDirectoryEntryBaseReloc,
        ImageDirectoryEntryDebug,
        ImageDirectoryEntryArchitecture,
        ImageDirectoryEntryGlobalPtr,
        ImageDirectoryEntryTls,
        ImageDirectoryEntryLoadConfig,
        ImageDirectoryEntryBoundImport,
        ImageDirectoryEntryIat,
        ImageDirectoryEntryDelayImport,
        ImageDirectoryEntryCOMDescriptor
    }
}
