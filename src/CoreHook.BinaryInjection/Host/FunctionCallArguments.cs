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

        private const int BinaryArgumentsSize = 12;

        private static Encoding PathEncoding = Encoding.Unicode;

        public FunctionCallArguments(IAssemblyDelegate assemblyDelegate, IBinarySerializer arguments)
        {
            if (assemblyDelegate == null)
            {
                throw new ArgumentNullException("Assembly delegate was null");
            }   

            Assembly = PathEncoding.GetBytes(assemblyDelegate.AssemblyName.PadRight(FunctionNameMax, '\0'));
            Class = PathEncoding.GetBytes(string.Format("{0}.{1}", 
                assemblyDelegate.AssemblyName, assemblyDelegate.TypeName).PadRight(FunctionNameMax, '\0'));
            Function = PathEncoding.GetBytes(assemblyDelegate.MethodName.PadRight(FunctionNameMax, '\0'));

            Arguments = arguments.Serialize();
        }
    }
}
