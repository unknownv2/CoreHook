


using System;
using System.Buffers;

using System.Collections.Generic;

using System.Linq;
using System.Runtime.InteropServices;

using System.Text;

namespace CoreHook.BinaryInjection.Memory;

public class MemoryManager : IDisposable
{
    private readonly List<CoreHook.BinaryInjection.Memory.MemoryAllocation> _memoryAllocations;

    private readonly SafeHandle _process;

    public IEnumerable<CoreHook.BinaryInjection.Memory.MemoryAllocation> Allocations => _memoryAllocations.AsReadOnly();

    public MemoryManager(SafeHandle process)
    {
        _process = process ?? throw new ArgumentNullException(nameof(process));
        _memoryAllocations = new List<CoreHook.BinaryInjection.Memory.MemoryAllocation>();
    }

    public CoreHook.BinaryInjection.Memory.MemoryAllocation Allocate(int size, CoreHook.BinaryInjection.Memory.MemoryProtectionType protection, bool mustBeDisposed = true)
    {
        var memory = new CoreHook.BinaryInjection.Memory.MemoryAllocation(_process, size, protection, mustBeDisposed);
        _memoryAllocations.Add(memory);
        return memory;
    }

    public void Deallocate(CoreHook.BinaryInjection.Memory.MemoryAllocation allocation)
    {
        if (!allocation.IsFree)
        {
            allocation.Dispose();
        }

        if (_memoryAllocations.Contains(allocation))
        {
            _memoryAllocations.Remove(allocation);
        }
    }

    public byte[] ReadMemory(long address)
    {
        throw new NotImplementedException();
    }


    public unsafe CoreHook.BinaryInjection.Memory.MemoryAllocation AllocateAndCopy<T>(T obj, bool mustBeDisposed = true)
    {
        int size;
        byte[] bytes;

        if (obj is string str)
        {
            bytes = Encoding.Unicode.GetBytes(str + "\0");
            size = bytes.Length;
        }
        else
        {
            nint handle = IntPtr.Zero;
            try
            {
                size = Marshal.SizeOf(obj);
                handle = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, handle, false);
                bytes = new Span<byte>((void*)handle, size).ToArray();
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        var argumentsAllocation = Allocate(size, CoreHook.BinaryInjection.Memory.MemoryProtectionType.ReadWrite, mustBeDisposed);

        argumentsAllocation.WriteBytes(bytes);

        return argumentsAllocation;
    }

    public virtual void Dispose()
    {
        foreach (var memoryAllocation in _memoryAllocations.Where(m => m.MustBeDisposed).ToArray())
        {
            memoryAllocation.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    ~MemoryManager() => Dispose();
}
