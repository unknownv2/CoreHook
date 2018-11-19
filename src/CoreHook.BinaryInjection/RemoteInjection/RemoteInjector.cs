﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using CoreHook.BinaryInjection.Loader;
using CoreHook.BinaryInjection.Loader.Serializer;
using CoreHook.BinaryInjection.Host;
using CoreHook.CoreLoad.Data;
using CoreHook.IPC.Platform;
using CoreHook.Memory;
using CoreHook.Memory.Processes;
using static CoreHook.BinaryInjection.ProcessUtils.ProcessHelper;

namespace CoreHook.BinaryInjection.RemoteInjection
{
    public static class RemoteInjector
    {
        /// <summary>
        /// The .NET Assembly class that loads the .NET hooking library, resolves any references, and executes
        /// the hooking library IEntryPoint.Run method.
        /// </summary>
        private static readonly AssemblyDelegate CoreHookLoaderDel =
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
                var managedProcess = new ManagedProcess(process);
                return new BinaryLoader(
                    new ProcessManager(managedProcess,
                    new MemoryManager(managedProcess)));
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
            return new BinaryLoaderConfig();
        }

        /// <summary>
        /// Get the name of the function that starts CoreCLR in a target process
        /// </summary>
        /// <returns>The name of the library function used to start CoreCLR.</returns>
        private static string GetCoreCLRStartFunctionName()
        {
            return BinaryLoaderHostConfig.CoreCLRStartFunction;
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
            return BinaryLoaderHostConfig.CoreCLRExecuteManagedFunction;
        }

        /// <summary>
        /// Create a process, inject the .NET Core runtime into it and load a .NET assembly.
        /// </summary>
        /// <param name="processConfig"></param>
        /// <param name="config32">Native modules required for starting CoreCLR in 32-bit applications.</param>
        /// <param name="config64">Native modules required for starting CoreCLR in 64-bit applications.</param>
        /// <param name="remoteInjectorConfig">Configuration settings for starting CoreCLR and executing .NET assemblies.</param>
        /// <param name="pipePlatform">Class for creating pipes for communication with the target process.</param>
        /// <param name="outProcessId">Process ID of the newly created process.</param>
        /// <param name="passThruArguments">Arguments passed to the .NET hooking library in the target process.</param>
        public static void CreateAndInject(
            ProcessCreationConfig processConfig,
            CoreHookNativeConfig config32,
            CoreHookNativeConfig config64,
            RemoteInjectorConfig remoteInjectorConfig,
            IPipePlatform pipePlatform,
            out int outProcessId,
            params object[] passThruArguments
            )
        {
            var process = Process.Start(processConfig.ExecutablePath);
            if (process == null)
            {
                throw new InvalidOperationException(
                    $"Failed to start the executable at {processConfig.ExecutablePath}");
            }

            var config = process.Is64Bit() ? config64 : config32;
            
            remoteInjectorConfig.HostLibrary = config.HostLibrary;
            remoteInjectorConfig.CoreCLRPath = config.CoreCLRPath;
            remoteInjectorConfig.CoreCLRLibrariesPath = config.CoreCLRLibrariesPath;
            remoteInjectorConfig.DetourLibrary = config.DetourLibrary;

            InjectEx(
                GetCurrentProcessId(),
                process.Id,
                remoteInjectorConfig,
                pipePlatform,
                passThruArguments);

            outProcessId = process.Id;
        }

        /// <summary>
        /// Start CoreCLR and execute a .NET assembly in a target process.
        /// </summary>
        /// <param name="targetPID">The process ID of the process to inject the .NET assembly into.</param>
        /// <param name="remoteInjectorConfig">Configuration settings for starting CoreCLR and executing .NET assemblies.</param>
        /// <param name="pipePlatform">Class for creating pipes for communication with the target process.</param>
        /// <param name="passThruArguments">Arguments passed to the .NET hooking library in the target process.</param>
        public static void Inject(
            int targetPID,
            RemoteInjectorConfig remoteInjectorConfig,
            IPipePlatform pipePlatform,
            params object[] passThruArguments)
        {
            InjectEx(
                GetCurrentProcessId(),
                targetPID,
                remoteInjectorConfig,
                pipePlatform,
                passThruArguments);
        }

        /// <summary>
        /// Start CoreCLR and execute a .NET assembly in a target process.
        /// </summary>
        /// <param name="hostPID">Process ID of the process communicating with the target process.</param>
        /// <param name="targetPID">The process ID of the process to inject the .NET assembly into.</param>
        /// <param name="remoteInjectorConfig">Configuration settings for starting CoreCLR and executing .NET assemblies.</param>
        /// <param name="pipePlatform">Class for creating pipes for communication with the target process.</param>
        /// <param name="passThruArguments">Arguments passed to the .NET hooking library in the target process.</param>
        public static void InjectEx(
            int hostPID,
            int targetPID,
            RemoteInjectorConfig remoteInjectorConfig,
            IPipePlatform pipePlatform,
            params object[] passThruArguments)
        {
            string injectionPipeName = remoteInjectorConfig.InjectionPipeName;
            if(string.IsNullOrWhiteSpace(injectionPipeName))
            {
                throw new ArgumentException("Invalid injection pipe name");
            }

            InjectionHelper.BeginInjection(targetPID);
            
            using (InjectionHelper.CreateServer(injectionPipeName, pipePlatform))
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
                        var libraryPath = remoteInjectorConfig.PayloadLibrary;
                        PrepareInjection(
                            remoteInfo,
                            new UserDataBinaryFormatter(),
                            libraryPath,
                            passThruStream,
                            injectionPipeName);

                        // Inject the CoreCLR hosting module into the process, start the CoreCLR
                        // and use the CoreLoad dll to resolve the dependencies of the hooking library
                        // and then call the IEntryPoint.Run method located in the hooking library
                        try
                        {
                            var process = GetProcessById(targetPID);
                            var length = (int)passThruStream.Length;

                            using (var binaryLoader = GetBinaryLoader(process))
                            {
                                binaryLoader.Load(remoteInjectorConfig.HostLibrary, new[] { remoteInjectorConfig.DetourLibrary });
                                binaryLoader.ExecuteRemoteFunction(
                                    new RemoteFunctionCall
                                    {
                                        Arguments = new BinaryLoaderSerializer(GetBinaryLoaderConfig())
                                        {
                                            Arguments = new BinaryLoaderArguments
                                            {
                                                Verbose = remoteInjectorConfig.VerboseLog,
                                                PayloadFileName = remoteInjectorConfig.CLRBootstrapLibrary,
                                                CoreRootPath = remoteInjectorConfig.CoreCLRPath,
                                                CoreLibrariesPath = remoteInjectorConfig.CoreCLRLibrariesPath
                                            }
                                        },
                                        FunctionName = new FunctionName
                                        { Module = remoteInjectorConfig.HostLibrary, Function = GetCoreCLRStartFunctionName() },
                                    });
                                binaryLoader.ExecuteRemoteManagedFunction(
                                new RemoteManagedFunctionCall
                                {
                                    ManagedFunction = CoreHookLoaderDel,
                                    FunctionName = new FunctionName
                                    { Module = remoteInjectorConfig.HostLibrary, Function = GetCoreCLRExecuteManagedFunctionName() },
                                    Arguments = new RemoteFunctionArguments
                                    {
                                        Is64BitProcess = process.Is64Bit(),
                                        UserData = binaryLoader.CopyMemoryTo(passThruStream.GetBuffer(), length),
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
        /// <param name="serializer">Serializes the <paramref name="remoteInfo"/> data.</param>
        /// <param name="library">The managed hooking library to be loaded and executed in the target process.</param>
        /// <param name="argumentsStream">The stream that holds the the serialized <paramref name="remoteInfo"/> class.</param>
        /// <param name="injectionPipeName">The pipe name used for notifying the host process that the hook plugin has been loaded in the target process.</param>
        private static void PrepareInjection(
            ManagedRemoteInfo remoteInfo,
            IUserDataFormatter serializer,
            string library,
            MemoryStream argumentsStream,
            string injectionPipeName)
        {
            if (string.IsNullOrWhiteSpace(library))
            {
                throw new ArgumentException("The injection library was not valid");
            }

            if (File.Exists(library))
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

            serializer.Serialize(argumentsStream, remoteInfo);
        }
    }
}
