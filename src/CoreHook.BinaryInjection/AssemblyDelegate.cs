using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.BinaryInjection
{
    public interface IAssemblyDelegate
    {
        string AssemblyName { get; }
        string TypeName { get; }
        string MethodName { get; }
    }
    public class AssemblyDelegate : IAssemblyDelegate
    {
        public string AssemblyName { get; }
        public string TypeName { get; }
        public string MethodName { get; }
        public AssemblyDelegate(string assemblyName, string typeName, string methodName)
        {
            AssemblyName = assemblyName;
            TypeName = typeName;
            MethodName = methodName;
        }
        public override string ToString()
        {
            return $"{AssemblyName}.{TypeName}.{MethodName}";
        }
    }
}
