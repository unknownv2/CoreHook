using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CoreHook.Unmanaged;
using System.IO;

namespace CoreHook.BinaryInjection
{
    [StructLayout(LayoutKind.Explicit)]
    public struct LinuxFunctionCallArgs
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMax)]
        public byte[] Assembly;

        [FieldOffset(256)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMax)]
        public byte[] Class;

        [FieldOffset(512)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMax)]
        public byte[] Function;

        [FieldOffset(768)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = BinaryArgumentsSize)]
        public byte[] Arguments;

        private const int FunctionNameMax = 256;
        private const int BinaryArgumentsSize = 12;

        public LinuxFunctionCallArgs(string classFunctionName, IntPtr arguments)
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

            Assembly = Encoding.ASCII.GetBytes(assembly.PadRight(FunctionNameMax, '\0'));
            Class = Encoding.ASCII.GetBytes(string.Format("{0}.{1}", assembly, type).PadRight(FunctionNameMax, '\0'));
            Function = Encoding.ASCII.GetBytes(function.PadRight(FunctionNameMax, '\0'));
            Arguments = Binary.StructToByteArray(arguments, BinaryArgumentsSize);
        }
        public LinuxFunctionCallArgs(string classFunctionName, RemoteFunctionArgs arguments)
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

            Assembly = Encoding.ASCII.GetBytes(assembly.PadRight(FunctionNameMax, '\0'));
            Class = Encoding.ASCII.GetBytes(string.Format("{0}.{1}", assembly, type).PadRight(FunctionNameMax, '\0'));
            Function = Encoding.ASCII.GetBytes(function.PadRight(FunctionNameMax, '\0'));
            Arguments = Binary.StructToByteArray(arguments, BinaryArgumentsSize);
        }
        public LinuxFunctionCallArgs(string classFunctionName, IBinarySerializer arguments)
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

            Assembly = Encoding.ASCII.GetBytes(assembly.PadRight(FunctionNameMax, '\0'));
            Class = Encoding.ASCII.GetBytes(string.Format("{0}.{1}", assembly, type).PadRight(FunctionNameMax, '\0'));
            Function = Encoding.ASCII.GetBytes(function.PadRight(FunctionNameMax, '\0'));
            Arguments = arguments.Serialize();
        }
    }
}
