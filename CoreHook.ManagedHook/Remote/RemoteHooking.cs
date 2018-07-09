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
        /// <summary>
        /// All supported options that will influence the way your library is injected.
        /// </summary>
        [Flags]
        public enum InjectionOptions
        {
            /// <summary>
            /// Default injection procedure.
            /// </summary>
            Default = 0x0,

            /// <summary>
            /// Use of services is not permitted.
            /// </summary>
            NoService = 0x1,

            /// <summary>
            /// Use of WOW64 bypass is not permitted.
            /// </summary>
            NoWOW64Bypass = 0x2,

            /// <summary>
            /// Allow injection without a strong name (e.g. no GAC registration). This option requires that the full path to injected assembly be provided
            /// </summary>
            DoNotRequireStrongName = 0x4,
        }

        /// <summary>
        /// See <see cref="Inject(Int32, InjectionOptions, String, String, Object[])"/> for more information.
        /// </summary>
        /// <param name="InTargetPID">
        /// The target process ID.
        /// </param>
        /// <param name="InLibraryPath_x86">
        /// A partially qualified assembly name or a relative/absolute file path of the 32-bit version of your library. 
        /// For example "MyAssembly, PublicKeyToken=248973975895496" or ".\Assemblies\MyAssembly.dll". 
        /// </param>
        /// <param name="InLibraryPath_x64">
        /// A partially qualified assembly name or a relative/absolute file path of the 64-bit version of your library. 
        /// For example "MyAssembly, PublicKeyToken=248973975895496" or ".\Assemblies\MyAssembly.dll". 
        /// </param>
        /// <param name="InPassThruArgs">
        /// A serializable list of parameters being passed to your library entry points <c>Run()</c> and
        /// constructor (see <see cref="IEntryPoint"/>).
        /// </param>
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
        /// <summary>
        /// Creates a new process which is started suspended until you call <see cref="WakeUpProcess"/>
        /// from within your injected library <c>Run()</c> method. This allows you to hook the target
        /// BEFORE any of its usual code is executed. In situations where a target has debugging and
        /// hook preventions, you will get a chance to block those mechanisms for example...
        /// </summary>
        /// <remarks>
        /// <para>
        /// Please note that this method might fail when injecting into managed processes, especially
        /// when the target is using the CLR hosting API and takes advantage of AppDomains. For example,
        /// the Internet Explorer won't be hookable with this method. In such a case your only options
        /// are either to hook the target with the unmanaged API or to hook it after (non-supended) creation 
        /// with the usual <see cref="Inject"/> method.
        /// </para>
        /// <para>
        /// See <see cref="Inject"/> for more information. The exceptions listed here are additional
        /// to the ones listed for <see cref="Inject"/>.
        /// </para>
        /// </remarks>
        /// <param name="InEXEPath">
        /// A relative or absolute path to the desired executable.
        /// </param>
        /// <param name="InCommandLine">
        /// Optional command line parameters for process creation.
        /// </param>
        /// <param name="InProcessCreationFlags">
        /// Internally CREATE_SUSPENDED is already passed to CreateProcess(). With this
        /// parameter you can add more flags like DETACHED_PROCESS, CREATE_NEW_CONSOLE or
        /// whatever!
        /// </param>
        /// <param name="InOptions">
        /// A valid combination of options.
        /// </param>
        /// <param name="InLibraryPath_x86">
        /// A partially qualified assembly name or a relative/absolute file path of the 32-bit version of your library. 
        /// For example "MyAssembly, PublicKeyToken=248973975895496" or ".\Assemblies\\MyAssembly.dll". 
        /// </param>
        /// <param name="InLibraryPath_x64">
        /// A partially qualified assembly name or a relative/absolute file path of the 64-bit version of your library. 
        /// For example "MyAssembly, PublicKeyToken=248973975895496" or ".\Assemblies\\MyAssembly.dll". 
        /// </param>
        /// <param name="OutProcessId">
        /// The process ID of the newly created process.
        /// </param>
        /// <param name="InPassThruArgs">
        /// A serializable list of parameters being passed to your library entry points <c>Run()</c> and
        /// constructor (see <see cref="IEntryPoint"/>).
        /// </param>
        /// <exception cref="ArgumentException">
        /// The given EXE path could not be found.
        /// </exception>
        public static void CreateAndInject(
            string InEXEPath,
            string coreHookDll,
            string coreRunDll,
            string coreLoadDll,
            string coreClrPath,
            string coreLibrariesPath,
            string InCommandLine,
            int InProcessCreationFlags,
            InjectionOptions InOptions,
            string InLibraryPath_x86,
            string InLibraryPath_x64,
            out int OutProcessId,
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
                0
                    //NativeMethods.CreateProcessFlags.CREATE_SUSPENDED | 
                    //NativeMethods.CreateProcessFlags.CREATE_DEFAULT_ERROR_MODE
                )
                ,
                IntPtr.Zero,
                null,
                ref si,
                out pi,
                coreHookDll,
                //ProcessHelper.Is64Bit ? InLibraryPath_x64 : InLibraryPath_x86,
                IntPtr.Zero
                ))
            {
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
                    null,
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
