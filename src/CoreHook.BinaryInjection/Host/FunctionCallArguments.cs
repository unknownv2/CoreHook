using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.BinaryInjection.Host
{
    [StructLayout(LayoutKind.Explicit)]
    public sealed partial class FunctionCallArguments
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMaxWide)]
        public byte[] Assembly;

        [FieldOffset(512)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMaxWide)]
        public byte[] Class;

        [FieldOffset(1024)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMaxWide)]
        public byte[] Function;

        [FieldOffset(1536)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = BinaryArgumentsSize)]
        public byte[] Arguments;

        private const int FunctionNameMax = 256;
        private const int FunctionNameMaxWide = FunctionNameMax*2;
        private const char PaddingCharacter = '\0';

        private const int BinaryArgumentsSize = 12;

        private static Encoding PathEncoding = Encoding.Unicode;

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
