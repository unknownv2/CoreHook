using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using CoreHook.BinaryInjection;
using CoreHook.ManagedHook.ProcessUtils;
using CoreHook.Unmanaged;
using CoreHook.CoreLoad;
using System.Threading;

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
        public static void Inject(
            int InTargetPID,
            string library)
        {
            using (var binaryLoader = GetBinaryLoader())
            {
                binaryLoader.Load(ProcessHelper.GetProcessById(InTargetPID), library);
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
            IPC.Platform.IPipePlatform pipePlatform,
            IEnumerable<string> dependencies,
            params object[] InPassThruArgs)
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
                    InPassThruArgs);
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
            IPC.Platform.IPipePlatform pipePlatform,
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
            IPC.Platform.IPipePlatform pipePlatform,
            IEnumerable<string> dependencies,
            params object[] InPassThruArgs)
        {
            MemoryStream PassThru = new MemoryStream();
            InjectionHelper.BeginInjection(targetPID);
            using (var pipeServer = InjectionHelper.CreateServer(InjectionPipe, pipePlatform))
            {
                try
                {
                    ManagedRemoteInfo RemoteInfo = new ManagedRemoteInfo();
                    RemoteInfo.HostPID = hostPID;

                    BinaryFormatter format = new BinaryFormatter();
                    List<object> args = new List<object>();
                    if (InPassThruArgs != null)
                    {
                        foreach (var arg in InPassThruArgs)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                format.Serialize(ms, arg);
                                args.Add(ms.ToArray());
                            }
                        }
                    }
                    RemoteInfo.UserParams = args.ToArray();

                    GCHandle hPassThru = PrepareInjection(
                        RemoteInfo,
                        ref lbraryPath_x86,
                        ref libraryPath_x64,
                        PassThru);

                    // Start library injection
                    try
                    {
                        var proc = ProcessHelper.GetProcessById(targetPID);
                        var length = (uint)PassThru.Length;

                        using (var binaryLoader = GetBinaryLoader())
                        {
                            Encoding encoding = null;
                            int pathLength = -1;
                            Object binaryLoaderArgs = null;
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            {
                                encoding = Encoding.ASCII;
                                pathLength = 4096;
                                binaryLoaderArgs = new LinuxBinaryLoaderArgs()
                                {
                                    Verbose = true,
                                    WaitForDebugger = false,
                                    StartAssembly = false,
                                    PayloadFileName = encoding.GetBytes(coreLoadDll.PadRight(pathLength, '\0')),
                                    CoreRootPath = encoding.GetBytes(coreClrPath.PadRight(pathLength, '\0')),
                                    CoreLibrariesPath = encoding.GetBytes(coreLibrariesPath.PadRight(pathLength, '\0'))
                                };

                            }
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                encoding = Encoding.Unicode;
                                pathLength = 260;
                                binaryLoaderArgs = new BinaryLoaderArgs()
                                {
                                    Verbose = true,
                                    WaitForDebugger = false,
                                    StartAssembly = false,
                                    PayloadFileName = encoding.GetBytes(coreLoadDll.PadRight(pathLength, '\0')),
                                    CoreRootPath = encoding.GetBytes(coreClrPath.PadRight(pathLength, '\0')),
                                    CoreLibrariesPath = encoding.GetBytes(coreLibrariesPath.PadRight(pathLength, '\0'))
                                };
                            }
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                            {
                                encoding = Encoding.ASCII;
                                pathLength = 1024;
                                binaryLoaderArgs = new MacOSBinaryLoaderArgs()
                                {
                                    Verbose = true,
                                    WaitForDebugger = false,
                                    StartAssembly = false,
                                    PayloadFileName = encoding.GetBytes(coreLoadDll.PadRight(pathLength, '\0')),
                                    CoreRootPath = encoding.GetBytes(coreClrPath.PadRight(pathLength, '\0')),
                                    CoreLibrariesPath = encoding.GetBytes(coreLibrariesPath.PadRight(pathLength, '\0'))
                                };
                            }
                            binaryLoader.Load(proc, coreRunDll, dependencies);
                            var argsAddr = binaryLoader.CopyMemoryTo(proc, PassThru.GetBuffer(), length);

                            //binaryLoader.ExecuteWithArgs(proc, coreRunDll, binaryLoaderArgs);

                            binaryLoader.CallFunctionWithRemoteArgs(proc,
                                coreRunDll,
                                CoreHookLoaderMethodName,
                                new RemoteFunctionArgs()
                                {
                                    UserData = argsAddr,
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
            ManagedRemoteInfo InRemoteInfo,
            ref String InLibraryPath_x86,
            ref String InLibraryPath_x64,
            MemoryStream InPassThruStream)
        {
            if (String.IsNullOrEmpty(InLibraryPath_x86) && String.IsNullOrEmpty(InLibraryPath_x64))
                throw new ArgumentException("At least one library for x86 or x64 must be provided");

            // ensure full path information in case of file names...
            if ((InLibraryPath_x86 != null) && File.Exists(InLibraryPath_x86))
                InLibraryPath_x86 = Path.GetFullPath(InLibraryPath_x86);

            if ((InLibraryPath_x64 != null) && File.Exists(InLibraryPath_x64))
                InLibraryPath_x64 = Path.GetFullPath(InLibraryPath_x64);

            // validate assembly type
            InRemoteInfo.UserLibrary = InLibraryPath_x86;

            if (ProcessHelper.Is64Bit)
                InRemoteInfo.UserLibrary = InLibraryPath_x64;

            if (File.Exists(InRemoteInfo.UserLibrary))
            {
                // translate to assembly name
                InRemoteInfo.UserLibraryName = AssemblyName.GetAssemblyName(InRemoteInfo.UserLibrary).FullName;
            }
            else
            {
                throw new FileNotFoundException(String.Format("The given assembly could not be found. {0}", InRemoteInfo.UserLibrary), InRemoteInfo.UserLibrary);
            }

            InRemoteInfo.ChannelName = InjectionPipe;

            var Format = new BinaryFormatter();
            Format.Serialize(InPassThruStream, InRemoteInfo);

            return GCHandle.Alloc(InPassThruStream.GetBuffer(), GCHandleType.Pinned);
        }
    }
}
