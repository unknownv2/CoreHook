using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using CoreHook.BinaryInjection;
using CoreHook.BinaryInjection.BinaryLoader;
using CoreHook.BinaryInjection.BinaryLoader.Windows;
using CoreHook.BinaryInjection.Host;
using CoreHook.CoreLoad;
using CoreHook.IPC.Platform;
using CoreHook.Memory;
using CoreHook.Memory.Processes;
using static CoreHook.ManagedHook.ProcessUtils.ProcessHelper;

namespace CoreHook.ManagedHook.Remote
{
    public class RemoteHooking
    {
        /// <summary>
        /// The .NET Assembly class that loads the .NET hooking library, resolves any references, and executes
        /// the hooking library IEntryPoint.Run method.
        /// </summary>
        private static AssemblyDelegate CoreHookLoaderDel =
                new AssemblyDelegate(
                assemblyName: "CoreHook.CoreLoad",
                typeName: "Loader",
                methodName: "Load");

        /// <summary>
        /// Retrieve the class used to load binary modules in a process.
        /// </summary>
        /// <param name="process">The target process.</param>
        /// <returns>The class that handles binary handling.</returns>
        private static IBinaryLoader GetBinaryLoader(Process process)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new BinaryLoader(
                    new MemoryManager(),
                    new ProcessManager(process));
            }
            else
            {
                throw new PlatformNotSupportedException("Binary injection");
            }
        }

        /// <summary>
        /// Retrieve system information such as string path encoding and max path length.
        /// </summary>
        /// <returns>Configuration class with system information.</returns>
        private static IBinaryLoaderConfig GetBinaryLoaderConfig()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new BinaryLoaderConfig();
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

        /// <summary>
        /// Create a process, inject the .NET Core runtime into it and load a .NET assembly.
        /// </summary>
        /// <param name="processConfig"></param>
        /// <param name="configX86">Native modules required for starting CoreCLR in 32-bit applications.</param>
        /// <param name="configX64">Native modules required for starting CoreCLR in 64-bit applications.</param>
        /// <param name="remoteHook">Configuration settings for starting CoreCLR and executing .NET assemblies.</param>
        /// <param name="pipePlatform">Class for creating pipes for communication with the target process.</param>
        /// <param name="outProcessId">Process ID of the newly created process.</param>
        /// <param name="passThruArguments">Arguments passed to the .NET hooking library in the target process.</param>
        public static void CreateAndInject(
            ProcessCreationConfig processConfig,
            CoreHookNativeConfig configX86,
            CoreHookNativeConfig configX64,
            RemoteHookingConfig remoteHook,
            IPipePlatform pipePlatform,
            out int outProcessId,
            params object[] passThruArguments
            )
        {
            var process = Process.Start(processConfig.ExecutablePath);

            var is64BitProcess = process.Is64Bit();

            remoteHook.HostLibrary = is64BitProcess ? configX64.HostLibrary : configX86.HostLibrary;
            remoteHook.CoreCLRPath = is64BitProcess ? configX64.CoreCLRPath : configX86.CoreCLRPath;
            remoteHook.CoreCLRLibrariesPath = is64BitProcess ? configX64.CoreCLRLibrariesPath : configX86.CoreCLRLibrariesPath;
            remoteHook.DetourLibrary = is64BitProcess ? configX64.DetourLibrary : configX86.DetourLibrary;

            InjectEx(
                GetCurrentProcessId(),
                process.Id,
                remoteHook,
                pipePlatform,
                passThruArguments);

            outProcessId = process.Id;
        }

        /// <summary>
        /// Start CoreCLR and execute a .NET assembly in a target process.
        /// </summary>
        /// <param name="targetPID">The process ID of the process to inject the .NET assembly into.</param>
        /// <param name="remoteHookConfig">Configuration settings for starting CoreCLR and executing .NET assemblies.</param>
        /// <param name="pipePlatform">Class for creating pipes for communication with the target process.</param>
        /// <param name="passThruArguments">Arguments passed to the .NET hooking library in the target process.</param>
        public static void Inject(
            int targetPID,
            RemoteHookingConfig remoteHookConfig,
            IPipePlatform pipePlatform,
            params object[] passThruArguments)
        {
            InjectEx(
                GetCurrentProcessId(),
                targetPID,
                remoteHookConfig,
                pipePlatform,
                passThruArguments);
        }

        /// <summary>
        /// Start CoreCLR and execute a .NET assembly in a target process.
        /// </summary>
        /// <param name="hostPID">Process ID of the process communicating with the target process.</param>
        /// <param name="targetPID">The process ID of the process to inject the .NET assembly into.</param>
        /// <param name="remoteHookConfig">Configuration settings for starting CoreCLR and executing .NET assemblies.</param>
        /// <param name="pipePlatform">Class for creating pipes for communication with the target process.</param>
        /// <param name="passThruArguments">Arguments passed to the .NET hooking library in the target process.</param>
        public static void InjectEx(
            int hostPID,
            int targetPID,
            RemoteHookingConfig remoteHookConfig,
            IPipePlatform pipePlatform,
            params object[] passThruArguments)
        {
            string injectionPipeName = remoteHookConfig.InjectionPipeName;
            if(string.IsNullOrEmpty(injectionPipeName))
            {
                throw new ArgumentException("Invalid injection pipe name");
            }

            InjectionHelper.BeginInjection(targetPID);
            
            using (var pipeServer = InjectionHelper.CreateServer(injectionPipeName, pipePlatform))
            {
                try
                {
                    var remoteInfo = new ManagedRemoteInfo { HostPID = hostPID };

                    var format = new BinaryFormatter();
                    var arguments = new List<object>();
                    if (passThruArguments != null)
                    {
                        foreach (var arg in passThruArguments)
                        {
                            using (var ms = new MemoryStream())
                            {
                                format.Serialize(ms, arg);
                                arguments.Add(ms.ToArray());
                            }
                        }
                    }
                    remoteInfo.UserParams = arguments.ToArray();

                    using (var passThruStream = new MemoryStream())
                    {
                        var libraryPath = remoteHookConfig.PayloadLibrary;
                        PrepareInjection(
                            remoteInfo,
                            ref libraryPath,
                            passThruStream,
                            injectionPipeName);

                        // Inject the corerundll into the process, start the CoreCLR
                        // and use the CoreLoad dll to resolve the dependencies of the hooking library
                        // and then call the IEntryPoint.Run method located in the hooking library
                        try
                        {
                            var process = GetProcessById(targetPID);
                            var length = (int)passThruStream.Length;

                            using (var binaryLoader = GetBinaryLoader(process))
                            {
                                binaryLoader.Load(process, remoteHookConfig.HostLibrary, new[] { remoteHookConfig.DetourLibrary });
                                binaryLoader.ExecuteRemoteFunction(process,
                                    new RemoteFunctionCall
                                    {
                                        Arguments = new BinaryLoaderSerializer(GetBinaryLoaderConfig())
                                        {
                                            Arguments = new BinaryLoaderArguments
                                            {
                                                Verbose = remoteHookConfig.VerboseLog,
                                                WaitForDebugger = remoteHookConfig.WaitForDebugger,
                                                PayloadFileName = remoteHookConfig.CLRBootstrapLibrary,
                                                CoreRootPath = remoteHookConfig.CoreCLRPath,
                                                CoreLibrariesPath = remoteHookConfig.CoreCLRLibrariesPath
                                            }
                                        },
                                        FunctionName = new FunctionName
                                        { Module = remoteHookConfig.HostLibrary, Function = GetCoreCLRStartFunctionName() },
                                    });
                                binaryLoader.ExecuteRemoteManagedFunction(process,
                                new RemoteManagedFunctionCall()
                                {
                                    ManagedFunction = CoreHookLoaderDel,
                                    FunctionName = new FunctionName
                                    { Module = remoteHookConfig.HostLibrary, Function = GetCoreCLRExecuteManagedFunctionName() },
                                    Arguments = new RemoteFunctionArguments
                                    {
                                        Is64BitProcess = process.Is64Bit(),
                                        UserData = binaryLoader.CopyMemoryTo(process, passThruStream.GetBuffer(), length),
                                        UserDataSize = length
                                    }
                                }
                                );

                                InjectionHelper.WaitForInjection(targetPID);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }
                }
                finally
                {
                    InjectionHelper.EndInjection(targetPID);
                }
            }
        }
        /// <summary>
        /// Create the config class that is passed to the CLR bootstrap library to be loaded.
        /// The <paramref name="remoteInfo"/> holds information such as what hooking module to load.
        /// </summary>
        /// <param name="remoteInfo">The configuration that is serialized and passed to CoreLoad.</param>
        /// <param name="library">The managed hooking library to be loaded and executed in the target process.</param>
        /// <param name="argumentsStream">The stream that holds the the serialized <paramref name="remoteInfo"/> class.</param>
        /// <param name="injectionPipeName">The pipe name used for notifying the host process that the hook plugin has been loaded in the target process.</param>
        private static void PrepareInjection(
            ManagedRemoteInfo remoteInfo,
            ref string library,
            MemoryStream argumentsStream,
            string injectionPipeName)
        {
            if (string.IsNullOrEmpty(library))
            {
                throw new ArgumentException("At least one library to be injected must be provided");
            }

            if ((library != null) && File.Exists(library))
            {
                library = Path.GetFullPath(library);
            }

            remoteInfo.UserLibrary = library;

            if (File.Exists(remoteInfo.UserLibrary))
            {
                remoteInfo.UserLibraryName = AssemblyName.GetAssemblyName(remoteInfo.UserLibrary).FullName;
            }
            else
            {
                throw new FileNotFoundException($"The given assembly could not be found: '{remoteInfo.UserLibrary}'", remoteInfo.UserLibrary);
            }

            remoteInfo.ChannelName = injectionPipeName;

            var formatter = new BinaryFormatter();
            formatter.Serialize(argumentsStream, remoteInfo);
        }
    }
}
