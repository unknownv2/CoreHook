
namespace CoreHook
{
    /// <summary>
    /// Manages the list of threads that are intercepted for each detour.
    /// </summary>
    public interface IHookAccessControl
    {
        bool IsExclusive { get; }
        bool IsInclusive { get; }
        void SetInclusiveACL(int[] acl);
        void SetExclusiveACL(int[] acl);
        int[] GetEntries();
    }
}
