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
        /// <summary>
        /// The interface implemented by each plugin that we initialize.
        /// </summary>
        private const string EntryPointInterface = "CoreHook.IEntryPoint";
        /// <summary>
        /// The name of the first method called in each plugin after initializing the class.
        /// </summary>
        private const string EntryPointMethodName = "Run";

        /// <summary>
        /// Initialize the plugin dependencies and execute its entry point.
        /// </summary>
        /// <param name="remoteParameters">Parameters containing the plugin to load
        /// and the parameters to pass to it's entry point.</param>
        /// <returns></returns>
        public static int Load(IntPtr remoteParameters)
        {
            try
            {
                if (remoteParameters == IntPtr.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(remoteParameters), 
                        "Remote arguments address was zero");
                }

                var remoteInfoFormatter = new UserDataBinaryFormatter();
                var connection =
                    ConnectionData<RemoteEntryInfo, ManagedRemoteInfo>.LoadData(
                        remoteParameters, remoteInfoFormatter
                    );

                var resolver = new Resolver(
                    connection.RemoteInfo.UserLibrary);

                var paramArray = new object[1 + connection.RemoteInfo.UserParams.Length];

                paramArray[0] = connection.UnmanagedInfo;
                for (int i = 0; i < connection.RemoteInfo.UserParams.Length; ++i)
                {
                    paramArray[i + 1] = connection.RemoteInfo.UserParams[i];
                }
              
                DeserializeParameters(paramArray, remoteInfoFormatter);

                // Execute the plugin library's entry point and pass in the user arguments.
                LoadUserLibrary(
                    resolver.Assembly,
                    paramArray,
                    connection.RemoteInfo.ChannelName);

                return (int)connection.State;
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
            return (int)PluginInitializationState.Invalid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramArray"></param>
        /// <param name="formatter"></param>
        private static void DeserializeParameters(object[] paramArray, IUserDataFormatter formatter)
        {
            for (int i = 1; i < paramArray.Length; ++i)
            {
                using (Stream ms = new MemoryStream((byte[])paramArray[i]))
                {
                    paramArray[i] = formatter.Deserialize<object>(ms);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="paramArray"></param>
        /// <param name="helperPipeName"></param>
        private static void LoadUserLibrary(Assembly assembly, object[] paramArray, string helperPipeName)
        {
            Type entryPoint = FindEntryPoint(assembly);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="methodName"></param>
        /// <param name="paramArray"></param>
        /// <returns></returns>
        private static MethodInfo FindMatchingMethod(Type objectType, string methodName, object[] paramArray)
        {
            var methods = objectType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.Name == methodName && (paramArray == null || MethodMatchesParameters(method, paramArray)))
                {
                    return method;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="paramArray"></param>
        /// <returns></returns>
        private static bool MethodMatchesParameters(MethodBase method, object[] paramArray)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != paramArray.Length)
            {
                return false;
            }
            
            for (var i = 0; i < paramArray.Length; ++i)
            {
                if (!parameters[i].ParameterType.IsInstanceOfType(paramArray[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Invoke a class constructor with a list of parameters.
        /// </summary>
        /// <param name="objectType">The type who's constructor is called.</param>
        /// <param name="parameters">The parameters to pass to the class constructor.</param>
        /// <returns>The instance returned from calling the constructor.</returns>
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

        /// <summary>
        /// Notify the injecting process when injection has completed successfully
        /// and the plugin is about to be executed.
        /// </summary>
        /// <param name="pipeName">The notification pipe created by the remote process.</param>
        /// <param name="pid">The process ID to send in the notification message.</param>
        /// <returns>True if the injection completion notification was sent successfully.</returns>
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
                //LocalHook.Release();
            }
        }

        private static void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
