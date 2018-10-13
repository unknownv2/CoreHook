using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private const string InjectionPipe = "CoreHookInjection";

        private static IBinaryLoader GetBinaryLoader2(Process process)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsBinaryLoader(
                    new MemoryManager(),
                    new Unmanaged.Windows.ProcessManager(process));
            }
            else
            {
                throw new PlatformNotSupportedException("Binary injection");
            }
        }

        private static IBinaryLoaderConfig GetBinaryLoaderConfig()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsBinaryLoaderConfig();
            }
            else
            {
                throw new PlatformNotSupportedException("Binary injection");
            }
        }

        /// <summary>
        /// Get the name of the function that starts CoreCLR in a target process
        /// </summary>
        /// <returns>The name of the library function used to start CoreCLR.</returns>
        private static string GetCoreCLRStartFunctionName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "StartCoreCLR";
            }
            else
            {
                throw new PlatformNotSupportedException("Binary injection");
            }
        }

        /// <summary>
        /// Get the name of a function that executes a single function inside
        /// a .NET library loaded in a process, referenced by class name
        /// and function name.
        /// </summary>
        /// <returns>The name of the library function used to execute the .NET
        /// Bootstrapping module, CoreLoad.
        /// </returns>
        private static string GetCoreCLRExecuteManagedFunctionName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "ExecuteAssemblyFunction";
            }
            else
            {
                throw new PlatformNotSupportedException("Binary injection");
            }
        }

        public static void CreateAndInject(
            ProcessCreationConfig processConfig,
            CoreHookNativeConfig configX86,
            CoreHookNativeConfig configX64,
            RemoteHookingConfig remoteHook,
            IPipePlatform pipePlatform,
            out int outProcessId,
            params object[] passThruArgs
            )
        {
            var process = Process.Start(processConfig.ExecutablePath);

            var is64BitProcess = process.Is64Bit();

            remoteHook.HostLibrary = is64BitProcess ? configX64.HostLibrary : configX86.HostLibrary;
            remoteHook.CoreCLRPath = is64BitProcess ? configX64.CoreCLRPath : configX86.CoreCLRPath;
            remoteHook.CoreCLRLibrariesPath = is64BitProcess ? configX64.CoreCLRLibrariesPath : configX86.CoreCLRLibrariesPath;
            remoteHook.DetourLibrary = is64BitProcess ? configX64.DetourLibrary : configX86.DetourLibrary;

            InjectEx(
                ProcessHelper.GetCurrentProcessId(),
                process.Id,
                remoteHook,
                pipePlatform,
                passThruArgs);

            outProcessId = process.Id;
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
                    var remoteInfo = new ManagedRemoteInfo { HostPID = hostPID };

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

                    // Inject the corerundll into the process, start the CoreCLR
                    // and use the CoreLoad dll to resolve the dependencies of the hooking library
                    // and then call the IEntryPoint.Run method located in the hooking library
                    try
                    {
                        var process = ProcessHelper.GetProcessById(targetPID);
                        var length = (uint)passThru.Length;

                        using (var binaryLoader = GetBinaryLoader2(process))
                        {
                            binaryLoader.Load(process, config.HostLibrary, new[] { config.DetourLibrary });

                            binaryLoader.ExecuteRemoteFunction(process,
                                new RemoteFunctionCall
                                {
                                    Arguments = new BinaryLoaderSerializer(GetBinaryLoaderConfig())
                                    {
                                        Arguments = new BinaryLoaderArgs
                                        {
                                            Verbose = config.VerboseLog,
                                            WaitForDebugger = config.WaitForDebugger,
                                            PayloadFileName = config.CLRBootstrapLibrary,
                                            CoreRootPath = config.CoreCLRPath,
                                            CoreLibrariesPath = config.CoreCLRLibrariesPath
                                        }
                                    },
                                    FunctionName = new FunctionName
                                    { Module = config.HostLibrary, Function = GetCoreCLRStartFunctionName() },
                                });
                                binaryLoader.ExecuteRemoteManagedFunction(process, 
                                new RemoteManagedFunctionCall()
                                {
                                    ManagedFunction = CoreHookLoaderDel,
                                    FunctionName = new FunctionName
                                    { Module = config.HostLibrary, Function = GetCoreCLRExecuteManagedFunctionName() },
                                    Arguments = new RemoteFunctionArgs
                                    {
                                        Is64BitProcess = process.Is64Bit(),
                                        UserData = binaryLoader.CopyMemoryTo(process, passThru.GetBuffer(), length),
                                        UserDataSize = length
                                    }
                                }
                                );

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
