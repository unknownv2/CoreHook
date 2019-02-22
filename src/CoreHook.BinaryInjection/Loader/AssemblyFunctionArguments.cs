using System;
using System.IO;
using CoreHook.BinaryInjection.Loader.Configuration;
using CoreHook.BinaryInjection.Loader.Serialization;

namespace CoreHook.BinaryInjection.Loader
{
    public class AssemblyFunctionArguments : ISerializableObject
    {
        private readonly IAssemblyDelegate _assemblyDelegate;
        private readonly ISerializableObject _arguments;
        private readonly IStringEncodingConfiguration _stringEncodingConfig;

        private const int FunctionNameMax = 256;

        public AssemblyFunctionArguments(
            IStringEncodingConfiguration stringEncodingConfig,
            IAssemblyDelegate assemblyDelegate,
            ISerializableObject arguments)
        {
            _assemblyDelegate = assemblyDelegate ?? throw new ArgumentNullException(nameof(assemblyDelegate));
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            _stringEncodingConfig = stringEncodingConfig ?? throw new ArgumentNullException(nameof(stringEncodingConfig));
        }

        private byte[] FormatDelegateString(string name)
        {
            return _stringEncodingConfig.Encoding.GetBytes(
                name.PadRight(FunctionNameMax, _stringEncodingConfig.PaddingCharacter));
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Format the assembly delegate name.
                writer.Write(FormatDelegateString(_assemblyDelegate.AssemblyName));

                writer.Write(FormatDelegateString($"{_assemblyDelegate.AssemblyName}.{_assemblyDelegate.TypeName}"));

                writer.Write(FormatDelegateString(_assemblyDelegate.MethodName));

                // Serialize the assembly delegate's arguments.
                writer.Write(_arguments.Serialize());

                return ms.ToArray();
            }
        }
    }
}
