
namespace CoreHook.BinaryInjection.PortableExecutable;

internal enum ImageDirectoryEntry : ushort
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
