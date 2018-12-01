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
                // Format the assembly delegate name.
                writer.Write(_pathConfiguration.PathEncoding.GetBytes(
                    _assemblyDelegate.AssemblyName.PadRight(
                        FunctionNameMax, _pathConfiguration.PaddingCharacter)));

                writer.Write(_pathConfiguration.PathEncoding.GetBytes(
                    $"{_assemblyDelegate.AssemblyName}.{_assemblyDelegate.TypeName}".PadRight(
                        FunctionNameMax, _pathConfiguration.PaddingCharacter)));

                writer.Write(_pathConfiguration.PathEncoding.GetBytes(
                    _assemblyDelegate.MethodName.PadRight(
                        FunctionNameMax, _pathConfiguration.PaddingCharacter)));

                // Serialize the assembly delegate's arguments.
                writer.Write(_arguments.Serialize());

                return ms.ToArray();
            }
        }
    }
}
