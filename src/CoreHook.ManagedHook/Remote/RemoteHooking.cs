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
        private static AssemblyDelegate CoreHookLoaderDel =
                new AssemblyDelegate(
                assemblyName: "CoreHook.CoreLoad",
                typeName: "Loader",
                methodName: "Load");

        private static string CoreHookLoaderMethodName = CoreHookLoaderDel.ToString();

        private const string InjectionPipe = "CoreHookInjection";

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
            ProcessCreationConfig process,
            RemoteHookingConfig remoteHook,
            IPipePlatform pipePlatform,
            out int outProcessId,
            params object[] passThruArgs
            )
        {
            outProcessId = -1;

            var si = new NativeMethods.StartupInfo();
            var pi = new NativeMethods.ProcessInformation();

            si.wShowWindow = 1;

            if (Unmanaged.Windows.NativeAPI.DetourCreateProcessWithDllExW(
                    process.ExecutablePath,
                    process.CommandLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    process.ProcessCreationFlags |
                    (uint)
                    (
                    NativeMethods.CreateProcessFlags.CREATE_NEW_CONSOLE
                    ),
                    IntPtr.Zero,
                    null,
                    ref si,
                    out pi,
                    remoteHook.DetourLibrary,
                    IntPtr.Zero
                    ))
            {
                outProcessId = pi.dwProcessId;

                InjectEx(
                    ProcessHelper.GetCurrentProcessId(),
                    pi.dwProcessId,
                    remoteHook,
                    pipePlatform,
                    passThruArgs);
            }
        }

        public static void Inject(
            int targetPID,
            RemoteHookingConfig remoteHook,
            IPipePlatform pipePlatform,
            params object[] passThruArgs)
        {
            InjectEx(
                ProcessHelper.GetCurrentProcessId(),
                targetPID,
                remoteHook,
                pipePlatform,
                passThruArgs);
        }

        public static void InjectEx(
            int hostPID,
            int targetPID,
            RemoteHookingConfig config,
            IPipePlatform pipePlatform,
            params object[] passThruArgs)
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
                    if (passThruArgs != null)
                    {
                        foreach (var arg in passThruArgs)
                        {
                            using (var ms = new MemoryStream())
                            {
                                format.Serialize(ms, arg);
                                args.Add(ms.ToArray());
                            }
                        }
                    }
                    remoteInfo.UserParams = args.ToArray();

                    var libraryPath = config.PayloadLibrary;
                    GCHandle hPassThru = PrepareInjection(
                        remoteInfo,
                        ref libraryPath,
                        ref libraryPath,
                        passThru);

                    // Inject the corerundll into the process, start the CoreCLR runtime
                    // and use the CoreLoad dll to resolve the dependencies of the hooking library
                    // and then call the IEntryPoint.Run method located in the hooking library
                    try
                    {
                        var proc = ProcessHelper.GetProcessById(targetPID);
                        var length = (uint)passThru.Length;

                        using (var binaryLoader = GetBinaryLoader())
                        {
                            binaryLoader.Load(proc, config.HostLibrary, new[] { config.DetourLibrary });

                            binaryLoader.CallFunctionWithRemoteArgs(proc,
                                config.HostLibrary,
                                CoreHookLoaderMethodName,
                                new BinaryLoaderArgs()
                                {
                                    Verbose = config.VerboseLog,
                                    WaitForDebugger = config.WaitForDebugger,
                                    StartAssembly = config.StartAssembly,
                                    PayloadFileName = config.CLRBootstrapLibrary,
                                    CoreRootPath = config.CoreCLRPath,
                                    CoreLibrariesPath = config.CoreCLRLibrariesPath
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
            ref string libraryX86,
            ref string libraryX64,
            MemoryStream argsStream)
        {
            if (string.IsNullOrEmpty(libraryX86) && string.IsNullOrEmpty(libraryX64))
            {
                throw new ArgumentException("At least one library for x86 or x64 must be provided");
            }

            // ensure full path information in case of file names...
            if ((libraryX86 != null) && File.Exists(libraryX86))
            {
                libraryX86 = Path.GetFullPath(libraryX86);
            }

            if ((libraryX64 != null) && File.Exists(libraryX64))
            {
                libraryX64 = Path.GetFullPath(libraryX64);
            }

            // validate assembly type
            remoteInfo.UserLibrary = libraryX86;

            if (ProcessHelper.Is64Bit)
            {
                remoteInfo.UserLibrary = libraryX64;
            }

            if (File.Exists(remoteInfo.UserLibrary))
            {
                // translate to assembly name
                remoteInfo.UserLibraryName = AssemblyName.GetAssemblyName(remoteInfo.UserLibrary).FullName;
            }
            else
            {
                throw new FileNotFoundException($"The given assembly could not be found: '{remoteInfo.UserLibrary}'", remoteInfo.UserLibrary);
            }

            remoteInfo.ChannelName = InjectionPipe;

            var formatter = new BinaryFormatter();
            formatter.Serialize(argsStream, remoteInfo);

            return GCHandle.Alloc(argsStream.GetBuffer(), GCHandleType.Pinned);
        }
    }
}
