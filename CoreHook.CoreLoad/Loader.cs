using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using CoreHook.IPC.NamedPipes;

namespace CoreHook.CoreLoad
{
    /*
     * Dependencies for CoreLoad:
     
     * Newtonsoft.Json.dll
     * Microsoft.Extensions.DependencyModel.dll
     * Microsoft.DotNet.PlatformAbstractions.dll
    */

    public class Loader
    {
        private const string EntryPointInterface = "CoreHook.IEntryPoint";
        private const string EntryPointMethodName = "Run";

        public Loader()
        {

        }

        public static int LoadUnmanaged([MarshalAs(UnmanagedType.LPWStr)]String inParam)
        {
            return 0;
        }
        const long APPMODEL_ERROR_NO_PACKAGE = 15700L;
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetPackageFamilyName(IntPtr hProcess, ref uint packageFamilyNameLength, StringBuilder packageFamilyName);

        private static bool IsUwp()
        {
            int length = 1024;
            StringBuilder sb = new StringBuilder(length);
            int result = GetCurrentPackageFullName(ref length, sb);
            if (result != APPMODEL_ERROR_NO_PACKAGE)
            {
                return true;
            }
            return false;
        }
        public static int Load(string paramPtr)
        {
            if (paramPtr == null)
            {
                return 0;
            }
            var ptr = (IntPtr)Int64.Parse(paramPtr, System.Globalization.NumberStyles.HexNumber);

            var connection = ConnectionData.LoadData(ptr);

            var resolver = new Resolver(connection.RemoteInfo.UserLibrary);

            // Prepare parameter array.
            var paramArray = new object[1 + connection.RemoteInfo.UserParams.Length];
            // The next type cast is not redundant because the object needs to be an explicit IContext
            // when passed as a parameter to the IEntryPoint constructor and Run() methods.
            paramArray[0] = connection.UnmanagedInfo;
            for (int i = 0; i < connection.RemoteInfo.UserParams.Length; i++)
                paramArray[i + 1] = connection.RemoteInfo.UserParams[i];

            LoadUserLibrary(resolver.Assembly, paramArray, connection.RemoteInfo.ChannelName);

            return 0;
        }
        private static void LoadUserLibrary(Assembly assembly, object[] paramArray, string helperPipeName)
        {
            var entryPoint = FindEntryPoint(assembly);
            BinaryFormatter format = new BinaryFormatter();
            format.Binder = new AllowAllAssemblyVersionsDeserializationBinder(entryPoint.Assembly);
            for (int i = 1; i < paramArray.Length; i++)
            {
                using (MemoryStream ms = new MemoryStream((byte[])paramArray[i]))
                {
                    paramArray[i] = format.Deserialize(ms);
                }
            }

            var runMethod = FindMatchingMethod(entryPoint, EntryPointMethodName, paramArray);
            var instance = InitializeInstance(entryPoint, paramArray);
            SendInjectionComplete(helperPipeName, Process.GetCurrentProcess().Id);
            try
            {
                // After this it is safe to enter the Run() method, which will block until assembly unloading...
                // From now on the user library has to take care about error reporting!
                runMethod.Invoke(instance, BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding |
                                           BindingFlags.InvokeMethod, null, paramArray, null);
  
            }
            finally
            {
                //Release(entryPoint);
            }
        }

        public static bool SendInjectionComplete(string pipeName, int pid)
        {
            using (NamedPipeClient pipeClient = new NamedPipeClient(pipeName))
            {
                if (pipeClient.Connect())
                {
                    var request = new NamedPipeMessages.InjectionCompleteNotification(pid, true);
                    if (pipeClient.TrySendRequest(request.CreateMessage()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Type FindEntryPoint(Assembly assembly)
        {
            var exportedTypes = assembly.GetExportedTypes();
            foreach (TypeInfo type in exportedTypes)
            {
                if (type.GetInterface(EntryPointInterface) != null)
                {
                    return type;
                }
            }
            return null;
        }
        private static MethodInfo FindMatchingMethod(Type objectType, string methodName, object[] paramArray)
        {
            var methods = objectType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.Name == methodName
                    && (paramArray != null ? MethodMatchesParameters(method, paramArray) : true))
                    return method;
            }
            return null;
        }
        private static bool MethodMatchesParameters(MethodBase method, object[] paramArray)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != paramArray.Length) return false;
            for (int i = 0; i < paramArray.Length; i++)
            {
                if (!parameters[i].ParameterType.IsInstanceOfType(paramArray[i]))
                    return false;
            }
            return true;
        }
        private static object InitializeInstance(Type objectType, object[] parameters)
        {
            var constructors = objectType.GetConstructors();
            foreach (var constructor in constructors)
            {
                if (MethodMatchesParameters(constructor, parameters))
                    return constructor.Invoke(parameters);
            }
            return null;
        }
    }
}
