using System;

namespace CoreHook.Unmanaged
{
    public class ModuleNotFoundException : Exception
    {
        public ModuleNotFoundException()
        {
            
        }

        public ModuleNotFoundException(string message) : base(message)
        {
            
        }
    }
}
