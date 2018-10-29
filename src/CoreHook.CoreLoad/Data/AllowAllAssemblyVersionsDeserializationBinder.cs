using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace CoreHook.CoreLoad.Data
{
    /// <summary>
    /// When not using the GAC, the BinaryFormatter fails to recognize the InParam
    /// when attempting to deserialize. 
    /// 
    /// A custom DeserializationBinder works around this (see http://spazzarama.com/2009/06/25/binary-deserialize-unable-to-find-assembly/)
    /// </summary>
    internal sealed class AllowAllAssemblyVersionsDeserializationBinder : SerializationBinder
    {
        private readonly Assembly _assembly;

        public AllowAllAssemblyVersionsDeserializationBinder()
            : this(Assembly.GetExecutingAssembly())
        {
        }

        public AllowAllAssemblyVersionsDeserializationBinder(Assembly assembly)
        {
            _assembly = assembly;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize;

            try
            {
                // 1. First try to bind without overriding assembly
                typeToDeserialize = Type.GetType($"{typeName}, {assemblyName}");
            }
            catch
            {
                // 2. Failed to find assembly or type, now try with overridden assembly
                typeToDeserialize = _assembly.GetType(typeName);
            }

            return typeToDeserialize;
        }
    }
}
