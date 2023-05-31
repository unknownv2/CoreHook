namespace CoreHook.HookDefinition;

/// <summary>
/// Interface for a hooking class that manages installs and uninstalls of function detours.
/// It requires a defining a function delegate type for the function that is being detoured.
/// </summary>
/// <typeparam name="T">A type representing the target function's delegate or signature.</typeparam>
public interface IHook<out T> : IHook where T : class
{
    /// <summary>
    /// The original function address, called to bypass the detour.
    /// </summary>
    T Original { get; }
    /// <summary>
    /// The target address where the detour is installed to.
    /// </summary>
    T Target { get; }
}
