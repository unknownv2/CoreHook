using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Memory
{
    public partial class MemoryAllocationType
    {
        public const uint Commit = Interop.MemoryAllocationType.Commit;
        public const uint Reserve = Interop.MemoryAllocationType.Reserve;
    }
}
