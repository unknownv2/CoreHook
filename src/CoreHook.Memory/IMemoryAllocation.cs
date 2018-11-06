
namespace CoreHook.Memory
{
    public interface IMemoryAllocation : IPointer, IDisposableState
    {
        bool IsFree { get; }
        int Size { get; }
    }
}
