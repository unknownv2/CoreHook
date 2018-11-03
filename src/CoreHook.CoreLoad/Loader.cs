using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using CoreHook.CoreLoad.Data;
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

        public static int Load(IntPtr remoteParameters)
        {
            try
            {
                if (remoteParameters == null || remoteParameters == IntPtr.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(remoteParameters), 
                        "Remote arguments address was zero");
                }

                var remoteInfoFormatter = new UserDataBinaryFormatter();
                var connection =
                    ConnectionData<RemoteEntryInfo, ManagedRemoteInfo>.LoadData(
                        remoteParameters, remoteInfoFormatter
                    );

                var resolver = new Resolver(connection.RemoteInfo.UserLibrary);

                var paramArray = new object[1 + connection.RemoteInfo.UserParams.Length];

                paramArray[0] = connection.UnmanagedInfo;
                for (int i = 0; i < connection.RemoteInfo.UserParams.Length; ++i)
                {
                    paramArray[i + 1] = connection.RemoteInfo.UserParams[i];
                }

                // Execute the plugin's entry point and pass in the user arguments 
                LoadUserLibrary(
                    resolver.Assembly, 
                    paramArray, 
                    connection.RemoteInfo.ChannelName,
                    remoteInfoFormatter);
            }
            catch(ArgumentOutOfRangeException outOfRangeEx)
            {
                Log(outOfRangeEx.ToString());
                throw;
            }
            catch (Exception exception)
            {
                Log(exception.ToString());
            }
            return 0;
        }

        private static void LoadUserLibrary(Assembly assembly, object[] paramArray, string helperPipeName, IUserDataFormatter formatter)
        {
            Type entryPoint = FindEntryPoint(assembly);

            for (int i = 1; i < paramArray.Length; ++i)
            {
                using (Stream ms = new MemoryStream((byte[])paramArray[i]))
                {
                    paramArray[i] = formatter.Deserialize<object>(ms);
                }
            }

            MethodInfo runMethod = FindMatchingMethod(entryPoint, EntryPointMethodName, paramArray);
            if(runMethod == null)
            {
                throw new MissingMethodException($"Failed to find the function 'Run' in {assembly.FullName}");
            }

            var instance = InitializeInstance(entryPoint, paramArray);
            if (instance == null)
            {
                throw new MissingMethodException($"Failed to find the constructor {entryPoint.Name} in {assembly.FullName}");
            }

            SendInjectionComplete(helperPipeName, Process.GetCurrentProcess().Id);
            try
            {
                // Execute the CoreHook plugin entry point
                runMethod.Invoke(instance, BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding |
                                           BindingFlags.InvokeMethod, null, paramArray, null);
  
            }
            finally
            {
                Release(entryPoint);
            }
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
                    && (paramArray == null || MethodMatchesParameters(method, paramArray)))
                {
                    return method;
                }
            }
            return null;
        }

        private static bool MethodMatchesParameters(MethodBase method, object[] paramArray)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != paramArray.Length)
            {
                return false;
            }

            for (int i = 0; i < paramArray.Length; ++i)
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
                {
                    return constructor.Invoke(parameters);
                }
            }
            return null;
        }

        private static bool SendInjectionComplete(string pipeName, int pid)
        {
            using (var pipeClient = new NamedPipeClient(pipeName))
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

        private static void Release(Type entryPoint)
        {
            if (entryPoint != null)
            {
                LocalHook.Release();
            }
        }

        private static void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
