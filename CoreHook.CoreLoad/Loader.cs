using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Linq;

namespace CoreHook.CoreLoad
{
    public class Loader
    {
        public Loader()
        {

        }

        public static int LoadUnmanaged([MarshalAs(UnmanagedType.LPWStr)]String inParam)
        {
            return 0;
        }

        public static int Load(string paramPtr)
        {
            if (paramPtr == null)
            {
                return 0;
            }

            Debug.WriteLine($"CoreHook.CoreLoad.Load: {paramPtr}");

            var ptr = (IntPtr)Int64.Parse(paramPtr, System.Globalization.NumberStyles.HexNumber);

            Debug.WriteLine($"CoreHook.CoreLoad.Load: {ptr.ToInt64().ToString()}");

            var connection = ConnectionData.LoadData(ptr);

            Debug.WriteLine($"CoreHook.CoreLoad.Load: Library {connection.RemoteInfo.UserLibrary}");

            var resolver = new Resolver(connection.RemoteInfo.UserLibrary);

            //Thread.Sleep(500);

            LoadUserLibrary(resolver.Assembly, connection.RemoteInfo.UserParams);

            return 0;
        }
        private static void LoadUserLibrary(Assembly assembly, object[] paramArray)
        {
            var entryPoint = FindEntryPoint(assembly);
            //var paramArray = new object[2];
            //paramArray[0] = new object();
            //paramArray[1] = "CoreHook_Debug";

            var runMethod = FindMatchingMethod(entryPoint, "Run", paramArray);
            var instance = InitializeInstance(entryPoint, paramArray);
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

        private static Type FindEntryPoint(Assembly assembly)
        {
            var exportedTypes = assembly.GetExportedTypes();
            foreach (TypeInfo type in exportedTypes)
            {
                Console.WriteLine(type.Name);
                if (type.GetInterface("AssemblyResolver.Core.Library.IEntryPoint") != null)
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
