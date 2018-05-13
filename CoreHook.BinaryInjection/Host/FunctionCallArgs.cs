using CoreHook.Unmanaged;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.BinaryInjection
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FunctionCallArgs
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] Assembly;

        [FieldOffset(512)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] Class;

        [FieldOffset(1024)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] Function;

        [FieldOffset(1536)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] Arguments;

        public FunctionCallArgs(string classFunctionName, byte[] arguments)
        {
            var args = classFunctionName.Split('.');
            string assembly = "";
            var argsCount = args.Length - 2;
            for (var x = 0; x < argsCount; x++)
            {
        
                assembly += args[x];
                if (x != argsCount - 1)
                {
                    assembly += ".";
                }
            }
            var type = args[argsCount++];
            var function = args[argsCount];

            Assembly = Encoding.Unicode.GetBytes(assembly.PadRight(256, '\0'));
            Class = Encoding.Unicode.GetBytes(string.Format("{0}.{1}", assembly, type).PadRight(256, '\0'));
            Function = Encoding.Unicode.GetBytes(function.PadRight(256, '\0'));
            Arguments = arguments ?? new byte[512];
        }

        public FunctionCallArgs(string classFunctionName, IntPtr arguments)
        {
            var args = classFunctionName.Split('.');
            string assembly = "";
            var argsCount = args.Length - 2;
            for (var x = 0; x < argsCount; x++)
            {

                assembly += args[x];
                if (x != argsCount - 1)
                {
                    assembly += ".";
                }
            }
            var type = args[argsCount++];
            var function = args[argsCount];

            Assembly = Encoding.Unicode.GetBytes(assembly.PadRight(256, '\0'));
            Class = Encoding.Unicode.GetBytes(string.Format("{0}.{1}", assembly, type).PadRight(256, '\0'));
            Function = Encoding.Unicode.GetBytes(function.PadRight(256, '\0'));
            Arguments = Binary.StructToByteArray(arguments, 512);
        }
        public FunctionCallArgs(string assembly, string type, string function, byte[] arguments)
        {
            Assembly = Encoding.Unicode.GetBytes(assembly.PadRight(256, '\0'));
            Class = Encoding.Unicode.GetBytes(string.Format("{0}.{1}", assembly,type).PadRight(256, '\0'));
            Function = Encoding.Unicode.GetBytes(function.PadRight(256, '\0'));
            Arguments = arguments ?? new byte[512];
        }
    }
}
