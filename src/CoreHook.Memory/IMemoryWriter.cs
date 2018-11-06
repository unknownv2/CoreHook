
namespace CoreHook.Memory
{
    public interface IMemoryWriter
    {
        void WriteMemory(long address, byte[] data);
    }
}
