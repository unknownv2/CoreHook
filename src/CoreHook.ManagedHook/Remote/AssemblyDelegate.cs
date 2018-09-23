using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.ManagedHook.Remote
{
    internal class AssemblyDelegate
    {
        private readonly string _assemblyName;
        private readonly string _typeName;
        private readonly string _methodName;

        internal AssemblyDelegate(string assemblyName, string typeName, string methodName)
        {
            _assemblyName = assemblyName;
            _typeName = typeName;
            _methodName = methodName;
        }

        public override string ToString()
        {
            return $"{_assemblyName}.{_typeName}.{_methodName}";
        }
    }
}
