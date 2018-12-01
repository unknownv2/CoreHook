using System;
using System.IO;
using CoreHook.BinaryInjection.Loader.Serializers;

namespace CoreHook.BinaryInjection.Loader
{
    public class AssemblyFunctionArguments : IBinarySerializer
    {
        private readonly IAssemblyDelegate _assemblyDelegate;
        private readonly IBinarySerializer _arguments;
        private readonly IPathEncodingConfiguration _pathConfiguration;
        private const int FunctionNameMax = 256;

        public byte[] Assembly;

        public byte[] Class;

        public byte[] Function;

        public byte[] Arguments;

        public AssemblyFunctionArguments(
            IPathEncodingConfiguration pathConfig,
            IAssemblyDelegate assemblyDelegate,
            IBinarySerializer arguments)
        {
            _assemblyDelegate = assemblyDelegate ?? throw new ArgumentNullException(nameof(assemblyDelegate));
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            _pathConfiguration = pathConfig ?? throw new ArgumentNullException(nameof(pathConfig));
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                Assembly = _pathConfiguration.PathEncoding.GetBytes(_assemblyDelegate.AssemblyName.PadRight(FunctionNameMax, _pathConfiguration.PaddingCharacter));
                Class = _pathConfiguration.PathEncoding.GetBytes($"{_assemblyDelegate.AssemblyName}.{_assemblyDelegate.TypeName}".PadRight(FunctionNameMax, _pathConfiguration.PaddingCharacter));
                Function = _pathConfiguration.PathEncoding.GetBytes(_assemblyDelegate.MethodName.PadRight(FunctionNameMax, _pathConfiguration.PaddingCharacter));

                Arguments = _arguments.Serialize();

                writer.Write(Assembly);
                writer.Write(Class);
                writer.Write(Function);

                writer.Write(Arguments);

                return ms.ToArray();
            }
        }
    }
}
