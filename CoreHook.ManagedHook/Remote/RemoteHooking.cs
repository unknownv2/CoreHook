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

        private const string CoreHookInjectionHelperPipe = "CoreHookInjection";

        public static void Inject(
            int InTargetPID,
            string InLibraryPath_x86,
            string InLibraryPath_x64,
            params object[] InPassThruArgs)
        {
            InjectEx(
                ProcessHelper.GetCurrentProcessId(),
                InTargetPID,
                0,
                0x20000000,
                InLibraryPath_x86,
                InLibraryPath_x64,
                true,
                true,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                InPassThruArgs);
        }
        private static IBinaryLoader GetBinaryLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxBinaryLoader();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new MacOSBinaryLoader();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new BinaryLoader();
            }
            else
            {
                throw new Exception("Unsupported platform for binary injection");
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
            int InProcessCreationFlags,
            string InLibraryPath_x86,
            string InLibraryPath_x64,
            out int OutProcessId,
            IPC.Platform.IPipePlatform pipePlatform,
            params object[] InPassThruArgs)
        {
            OutProcessId = -1;
            var si = new NativeMethods.StartupInfo();
            var pi = new NativeMethods.ProcessInformation();

            if(Unmanaged.Windows.NativeAPI.DetourCreateProcessWithDllExW(InEXEPath,
                null,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                (uint)(InProcessCreationFlags) |
                (uint)
                (
                NativeMethods.CreateProcessFlags.CREATE_NEW_CONSOLE
                )
                ,
                IntPtr.Zero,
                null,
                ref si,
                out pi,
                coreHookDll,
                IntPtr.Zero
                ))
            {
                OutProcessId = pi.dwProcessId;

                InjectEx(
                    ProcessHelper.GetCurrentProcessId(),
                    pi.dwProcessId,
                    pi.dwThreadId,
                    0x20000000,
                    InLibraryPath_x86,
                    InLibraryPath_x64,
                    true,
                    true,
                    coreRunDll,
                    coreLoadDll,
                    coreClrPath,
                    coreLibrariesPath,
                    pipePlatform,
                    InPassThruArgs);
            }
            else
            {
                throw new Exception("failed to start processs");
            }
        }

        public static void Inject(
            int InTargetPID,
            string coreRunDll,
            string coreLoadDll,
            string coreClrPath,
            string coreLibrariesPath,
            string InLibraryPath_x86,
            string InLibraryPath_x64,
            IPC.Platform.IPipePlatform pipePlatform,
            params object[] InPassThruArgs)
        {
            InjectEx(
                ProcessHelper.GetCurrentProcessId(),
                InTargetPID,
                0,
                0x20000000,
                InLibraryPath_x86,
                InLibraryPath_x64,
                true,
                true,
                coreRunDll,
                coreLoadDll,
                coreClrPath,
                coreLibrariesPath,
                pipePlatform,
                InPassThruArgs);
        }

        internal static void InjectEx(
            int InHostPID,
            int InTargetPID,
            int InWakeUpTID,
            int InNativeOptions,
            string InLibraryPath_x86,
            string InLibraryPath_x64,
            bool InCanBypassWOW64,
            bool InRequireStrongName,
            string coreRunDll,
            string coreLoadDll,
            string coreClrPath,
            string coreLibrariesPath,
            IPC.Platform.IPipePlatform pipePlatform,
            params object[] InPassThruArgs)
        {
            MemoryStream PassThru = new MemoryStream();
            InjectionHelper.BeginInjection(InTargetPID);
            using (var pipeServer = InjectionHelper.CreateServer(CoreHookInjectionHelperPipe, pipePlatform))
            {
                try
                {
                    ManagedRemoteInfo RemoteInfo = new ManagedRemoteInfo();
                    RemoteInfo.HostPID = InHostPID;

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

                    RemoteInfo.RequireStrongName = InRequireStrongName;

                    GCHandle hPassThru = PrepareInjection(
                        RemoteInfo,
                        ref InLibraryPath_x86,
                        ref InLibraryPath_x64,
                        PassThru);

                    /*
                        Inject library...
                     */
                    try
                    {
                        var proc = ProcessHelper.GetProcessById(InTargetPID);
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
                            binaryLoader.Load(proc, coreRunDll);
                            var argsAddr = binaryLoader.CopyMemoryTo(proc, PassThru.GetBuffer(), length);

                            binaryLoader.ExecuteWithArgs(proc, coreRunDll, binaryLoaderArgs);

                            binaryLoader.CallFunctionWithRemoteArgs(proc,
                                coreRunDll,
                                CoreHookLoaderMethodName,
                                new RemoteFunctionArgs()
                                {
                                    UserData = argsAddr,
                                    UserDataSize = length
                                });

                            InjectionHelper.WaitForInjection(InTargetPID);
                        }
                    }
                    finally
                    {
                        hPassThru.Free();
                    }
                }
                finally
                {
                    InjectionHelper.EndInjection(InTargetPID);
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

            /*
                validate assembly name...
             */
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

            InRemoteInfo.ChannelName = CoreHookInjectionHelperPipe;

            /*
            // Attempt to load the library by its FullName and if that fails, by its original library filename
            Assembly UserAsm = null;
            try
            {
                if (!String.IsNullOrEmpty(InRemoteInfo.UserLibraryName))
                {
                    UserAsm = Assembly.ReflectionOnlyLoad(InRemoteInfo.UserLibraryName);
                }
            }
            catch (FileNotFoundException)
            {
                // We already know the file exists at this point so try to load from original library filename instead
                UserAsm = null;
            }
            if (UserAsm == null && (UserAsm = Assembly.ReflectionOnlyLoadFrom(InRemoteInfo.UserLibrary)) == null)
                throw new DllNotFoundException(String.Format("The given assembly could not be found. {0}", InRemoteInfo.UserLibrary));

            // Check for a strong name if necessary
            if (InRemoteInfo.RequireStrongName && (Int32)(UserAsm.GetName().Flags & AssemblyNameFlags.PublicKey) == 0)
                throw new ArgumentException("The given assembly has no strong name.");
            */
            /*
                Convert managed arguments to binary stream...
             */

            var Format = new BinaryFormatter();
            Format.Serialize(InPassThruStream, InRemoteInfo);

            return GCHandle.Alloc(InPassThruStream.GetBuffer(), GCHandleType.Pinned);
        }
    }
}
