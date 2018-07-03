using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CoreHook.Unmanaged;

namespace CoreHook.BinaryInjection
{
    [StructLayout(LayoutKind.Explicit)]
    public struct LinuxFunctionCallArgs
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] Assembly;

        [FieldOffset(256)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] Class;

        [FieldOffset(512)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] Function;

        [FieldOffset(768)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] Arguments;

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

            Assembly = Encoding.ASCII.GetBytes(assembly.PadRight(256, '\0'));
            Class = Encoding.ASCII.GetBytes(string.Format("{0}.{1}", assembly, type).PadRight(256, '\0'));
            Function = Encoding.ASCII.GetBytes(function.PadRight(256, '\0'));
            Arguments = Binary.StructToByteArray(arguments, 512);
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

            Assembly = Encoding.ASCII.GetBytes(assembly.PadRight(256, '\0'));
            Class = Encoding.ASCII.GetBytes(string.Format("{0}.{1}", assembly, type).PadRight(256, '\0'));
            Function = Encoding.ASCII.GetBytes(function.PadRight(256, '\0'));
            Arguments = Binary.StructToByteArray(arguments, 512);
        }
    }
}
