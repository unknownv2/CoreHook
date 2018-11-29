using System;
using System.Runtime.InteropServices;
using System.Text;
using CoreHook.BinaryInjection.Loader.Serializer;

namespace CoreHook.BinaryInjection.Host
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct FunctionCallArguments
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMaxWide)]
        public byte[] Assembly;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMaxWide)]
        public byte[] Class;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMaxWide)]
        public byte[] Function;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = BinaryArgumentsSize)]
        public byte[] Arguments;

        private const int FunctionNameMax = 256;
        private const int FunctionNameMaxWide = FunctionNameMax * 2;
        private const char PaddingCharacter = '\0';

        private const int BinaryArgumentsSize = 12;

        private static readonly Encoding PathEncoding = Encoding.Unicode;

        public FunctionCallArguments(IAssemblyDelegate assemblyDelegate, IBinarySerializer arguments)
        {
            if (assemblyDelegate == null)
            {
                throw new ArgumentNullException(nameof(assemblyDelegate));
            }

            Assembly = PathEncoding.GetBytes(assemblyDelegate.AssemblyName.PadRight(FunctionNameMax, PaddingCharacter));
            Class = PathEncoding.GetBytes($"{assemblyDelegate.AssemblyName}.{assemblyDelegate.TypeName}".PadRight(FunctionNameMax, PaddingCharacter));
            Function = PathEncoding.GetBytes(assemblyDelegate.MethodName.PadRight(FunctionNameMax, PaddingCharacter));

            Arguments = arguments.Serialize();
        }
    }
}
