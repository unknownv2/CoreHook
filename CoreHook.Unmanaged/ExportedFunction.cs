using System;

namespace CoreHook.Unmanaged
{
    public class ExportedFunction
    {
        public string Name { get; private set; }

        public IntPtr AbsoluteAddress { get; private set; }

        public ExportedFunction(string name, IntPtr absoluteAddress)
        {
            Name = name;
            AbsoluteAddress = absoluteAddress;
        }
    }
}
