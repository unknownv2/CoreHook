using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using CoreHook.BinaryInjection;
using CoreHook.CoreLoad;
using CoreHook.IPC.Platform;
using CoreHook.ManagedHook.ProcessUtils;
using CoreHook.Unmanaged;

namespace CoreHook.ManagedHook.Remote
{
    public class RemoteHooking
    {
        private const string CoreHookLoaderMethodName = "CoreHook.CoreLoad.Loader.Load";

        private const string InjectionPipe = "CoreHookInjection";

        public class UnsupportedPlatformException : Exception
        {
            public UnsupportedPlatformException(string operation)
                        : base($"Unsupported platform for {operation}.")
            {
            }
        }
        public class ProcessStartException : Exception
        {
            public ProcessStartException(string processName)
                        : base($"Failed to start process {processName}.")
            {
            }
        }

        private static IBinaryLoader GetBinaryLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxBinaryLoader(
                    new MemoryManager());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new MacOSBinaryLoader(
                    new MemoryManager());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new BinaryLoader(
                    new MemoryManager());
            }
            else
            {
                throw new UnsupportedPlatformException("Binary injection");
            }
        }
  
        public static void CreateAndInject(
            string InEXEPath,
            string coreHookDll,
            string coreRunDll,
            string coreLoadDll,
            string coreClrPath,
            string coreLibrariesPath,
            string InCommandLine,
            uint processCreationFlags,
            string lbraryPath_x86,
            string libraryPath_x64,
            out int outProcessId,
            IPipePlatform pipePlatform,
            IEnumerable<string> dependencies,
            params object[] passThruArgs)
        {
            outProcessId = -1;
            var si = new NativeMethods.StartupInfo();
            var pi = new NativeMethods.ProcessInformation();

            if(Unmanaged.Windows.NativeAPI.DetourCreateProcessWithDllExW(InEXEPath,
                InCommandLine,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                processCreationFlags |
                (uint)
                (
                NativeMethods.CreateProcessFlags.CREATE_NEW_CONSOLE
                ),
                IntPtr.Zero,
                null,
                ref si,
                out pi,
                coreHookDll,
                IntPtr.Zero
                ))
            {
                outProcessId = pi.dwProcessId;

                InjectEx(
                    ProcessHelper.GetCurrentProcessId(),
                    pi.dwProcessId,
                    pi.dwThreadId,
                    lbraryPath_x86,
                    libraryPath_x64,
                    true,
                    coreRunDll,
                    coreLoadDll,
                    coreClrPath,
                    coreLibrariesPath,
                    pipePlatform,
                    dependencies,
                    passThruArgs);
            }
            else
            {
                throw new ProcessStartException(InEXEPath);
            }
        }

        public static void Inject(
            int InTargetPID,
            string coreRunDll,
            string coreLoadDll,
            string coreClrPath,
            string coreLibrariesPath,
            string lbraryPath_x86,
            string libraryPath_x64,
            IPipePlatform pipePlatform,
            IEnumerable<string> dependencies,
            params object[] InPassThruArgs)
        {
            InjectEx(
                ProcessHelper.GetCurrentProcessId(),
                InTargetPID,
                0,
                lbraryPath_x86,
                libraryPath_x64,
                true,
                coreRunDll,
                coreLoadDll,
                coreClrPath,
                coreLibrariesPath,
                pipePlatform,
                dependencies,
                InPassThruArgs);
        }

        public static void InjectEx(
            int hostPID,
            int targetPID,
            int wakeUpTID,
            string lbraryPath_x86,
            string libraryPath_x64,
            bool InCanBypassWOW64,
            string coreRunDll,
            string coreLoadDll,
            string coreClrPath,
            string coreLibrariesPath,
            IPipePlatform pipePlatform,
            IEnumerable<string> dependencies,
            params object[] InPassThruArgs)
        {
            var passThru = new MemoryStream();
            InjectionHelper.BeginInjection(targetPID);
            using (var pipeServer = InjectionHelper.CreateServer(InjectionPipe, pipePlatform))
            {
                try
                {
                    var remoteInfo = new ManagedRemoteInfo();
                    remoteInfo.HostPID = hostPID;

                    var format = new BinaryFormatter();
                    var args = new List<object>();
                    if (InPassThruArgs != null)
                    {
                        foreach (var arg in InPassThruArgs)
                        {
                            using (var ms = new MemoryStream())
                            {
                                format.Serialize(ms, arg);
                                args.Add(ms.ToArray());
                            }
                        }
                    }
                    remoteInfo.UserParams = args.ToArray();

                    GCHandle hPassThru = PrepareInjection(
                        remoteInfo,
                        ref lbraryPath_x86,
                        ref libraryPath_x64,
                        passThru);

                    // Start library injection
                    try
                    {
                        var proc = ProcessHelper.GetProcessById(targetPID);
                        var length = (uint)passThru.Length;

                        using (var binaryLoader = GetBinaryLoader())
                        {              
                            binaryLoader.Load(proc, coreRunDll, dependencies);

                            binaryLoader.CallFunctionWithRemoteArgs(proc,
                                coreRunDll,
                                CoreHookLoaderMethodName,
                                new BinaryLoaderArgs()
                                {
                                    Verbose = true,
                                    WaitForDebugger = false,
                                    StartAssembly = false,
                                    PayloadFileName = coreLoadDll,
                                    CoreRootPath = coreClrPath,
                                    CoreLibrariesPath = coreLibrariesPath
                                },
                                new RemoteFunctionArgs()
                                {
                                    UserData = binaryLoader.CopyMemoryTo(proc, passThru.GetBuffer(), length),
                                    UserDataSize = length
                                });

                            InjectionHelper.WaitForInjection(targetPID);
                        }
                    }
                    finally
                    {
                        hPassThru.Free();
                    }
                }
                finally
                {
                    InjectionHelper.EndInjection(targetPID);
                }
            }
        }

        private static GCHandle PrepareInjection(
            ManagedRemoteInfo remoteInfo,
            ref String libraryX86,
            ref String libraryX64,
            MemoryStream argsStream)
        {
            if (String.IsNullOrEmpty(libraryX86) && String.IsNullOrEmpty(libraryX64))
                throw new ArgumentException("At least one library for x86 or x64 must be provided");

            // ensure full path information in case of file names...
            if ((libraryX86 != null) && File.Exists(libraryX86))
                libraryX86 = Path.GetFullPath(libraryX86);

            if ((libraryX64 != null) && File.Exists(libraryX64))
                libraryX64 = Path.GetFullPath(libraryX64);

            // validate assembly type
            remoteInfo.UserLibrary = libraryX86;

            if (ProcessHelper.Is64Bit)
                remoteInfo.UserLibrary = libraryX64;

            if (File.Exists(remoteInfo.UserLibrary))
            {
                // translate to assembly name
                remoteInfo.UserLibraryName = AssemblyName.GetAssemblyName(remoteInfo.UserLibrary).FullName;
            }
            else
            {
                throw new FileNotFoundException(String.Format("The given assembly could not be found. {0}", remoteInfo.UserLibrary), remoteInfo.UserLibrary);
            }

            remoteInfo.ChannelName = InjectionPipe;

            var formatter = new BinaryFormatter();
            formatter.Serialize(argsStream, remoteInfo);

            return GCHandle.Alloc(argsStream.GetBuffer(), GCHandleType.Pinned);
        }
    }
}
