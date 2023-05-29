
namespace CoreHook;

/// <summary>
/// Manages the list of threads that are intercepted for each detour.
/// </summary>
public interface IHookAccessControl
{
    /// <summary>
    /// True if the current thread ACL is exclusive, as described in
    /// <see cref="SetExclusiveACL"/>.
    /// </summary>
    bool IsExclusive { get; }
    /// <summary>
    /// True if the current thread ACL is inclusive, as described in 
    /// <see cref="SetInclusiveACL"/>.
    /// </summary>
    bool IsInclusive { get; }
    /// <summary>
    /// Overwrite the current ACL and set an inclusive list of threads.
    /// Inclusive means that all threads in <paramref name="acl"/> are
    /// intercepted and any others not present in the list are not.
    /// </summary>
    /// <param name="acl">A list of threads to that are intercepted.</param>
    void SetInclusiveACL(int[] acl);
    /// <summary>
    /// Overwrite the current ACL and set an exclusive list of threads.
    /// Exclusive means that all threads in <paramref name="acl"/> are
    /// not intercepted and all other threads are intercepted.
    /// </summary>
    /// <param name="acl">List of threads to exclude from intercepting.</param>
    void SetExclusiveACL(int[] acl);
    /// <summary>
    /// Get a copy of the current thread list for this ACL.
    /// The returned list can be edited without affecting the hook.
    /// </summary>
    /// <returns>A copy of the ACL's thread list.</returns>
    int[] GetEntries();
}
