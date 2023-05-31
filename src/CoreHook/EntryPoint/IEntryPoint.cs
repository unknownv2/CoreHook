namespace CoreHook.EntryPoint;

/// <summary>
/// Used for finding the entry point of a plugin to load.
/// Each plugin must have a class that inherits from this interface.
/// The class must have a method called 'Run', which is executed
/// when the module is being initialized.
/// </summary>
public interface IEntryPoint
{

}
