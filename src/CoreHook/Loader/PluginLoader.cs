using CoreHook.EntryPoint;
using CoreHook.IPC.Messages;

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CoreHook.Loader;

public class PluginLoader
{
    /// <summary>
    /// The interface implemented by each plugin that we initialize.
    /// </summary>
    private static string EntryPointInterface = typeof(IEntryPoint).FullName!;

    /// <summary>
    /// The name of the first method called in each plugin after initializing the class.
    /// </summary>
    private const string EntryPointMethodName = "Run";

    /// <summary>
    /// Initialize the plugin dependencies and execute its entry point.
    /// </summary>
    /// <param name="remoteInfoAddr">Parameters containing the plugin to load and the parameters to pass to it's entry point.</param>
    /// <returns>A status code representing the plugin initialization state.</returns>
    [UnmanagedCallersOnly]//(CallConvs = new[] { typeof(CallConvCdecl) })]
    public unsafe static int Load(IntPtr payLoadPtr)
    {
        try
        {
            if (payLoadPtr == IntPtr.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(payLoadPtr), "Remote arguments address was zero");
            }

            var payLoadStr = Marshal.PtrToStringUni(payLoadPtr);

            var payLoad = JsonSerializer.Deserialize<ManagedRemoteInfo>(payLoadStr, new JsonSerializerOptions() { IncludeFields = true });

            payLoad.UserParams = payLoad.UserParams?.Zip(payLoad.UserParamsTypeNames!, (param, typeName) => param is null ? null : ((JsonElement)param).Deserialize(Type.GetType(typeName, true))).ToArray() ?? Array.Empty<object>();

            // Start the IPC message notifier with a connection to the host application.
            using var hostNotifier = new NotificationHelper(payLoad.ChannelName);

            _ = hostNotifier.Log($"Initializing plugin: {payLoad.UserLibrary}.");

            //TODO: deps.json file is not copied to output! fix this
            var resolver = new DependencyResolver(payLoad.UserLibrary);

            // Execute the plugin library's entry point and pass in the user arguments.
            var t = LoadPlugin(resolver.Assembly, payLoad.UserParams, hostNotifier);
            t.Wait();
            return (int)t.Result;
        }
        catch (ArgumentOutOfRangeException outOfRangeEx)
        {
            Log(outOfRangeEx.ToString());
            throw;
        }
        catch (Exception e)
        {
            Log(e.ToString());
        }
        return (int)PluginInitializationState.Failed;
    }

    /// <summary>
    /// Find the entry point of the plugin module, initialize it, and execute its Run method.
    /// </summary>
    /// <param name="assembly">The plugin assembly containing the entry point.</param>
    /// <param name="paramArray">The parameters passed to the plugin Run method.</param>
    /// <param name="hostNotifier">Used to notify the host about the state of the plugin initialization.</param>
    private static async Task<PluginInitializationState> LoadPlugin(Assembly assembly, object[] paramArray, NotificationHelper hostNotifier)
    {
        var entryPoint = FindEntryPoint(assembly);
        if (entryPoint is null)
        {
            Log(hostNotifier, new ArgumentException($"Assembly {assembly.FullName} doesn't contain any exposed type implementing {EntryPointInterface}."));
        }

        var runMethod = FindMatchingMethod(entryPoint, EntryPointMethodName, paramArray);
        if (runMethod is null)
        {
            Log(hostNotifier, new MissingMethodException($"Failed to find the 'Run' function with {paramArray.Length} parameter(s) in {assembly.FullName}."));
        }

        _ = hostNotifier.Log("Found entry point, initializing plugin class.");

        var instance = InitializeInstance(entryPoint, paramArray);
        if (instance is null)
        {
            Log(hostNotifier, new MissingMethodException($"Failed to find the constructor {entryPoint.Name} in {assembly.FullName}"));
        }
        _ = hostNotifier.Log("Plugin successfully initialized. Executing the plugin entry point.");

        if (await hostNotifier.SendInjectionComplete(Environment.ProcessId))
        {
            // Close the plugin loading message channel.
            hostNotifier.Dispose();

            try
            {
                // Execute the plugin 'Run' entry point.
                runMethod?.Invoke(instance, BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding | BindingFlags.InvokeMethod, null, paramArray, null);
            }
            catch
            {
            }
            return PluginInitializationState.Initialized;
        }
        return PluginInitializationState.Failed;
    }

    /// <summary>
    /// Find the CoreHook entry point in the plugin module.
    /// </summary>
    /// <param name="assembly">The plugin module.</param>
    /// <returns>The CoreHook plugin entry point.</returns>
    private static Type? FindEntryPoint(Assembly assembly)
    {
        return assembly.GetExportedTypes()
                       .FirstOrDefault(type => type.GetInterface(EntryPointInterface) is not null);
    }

    /// <summary>
    /// Find a method from a type based on the name and parameters.
    /// </summary>
    /// <param name="objectType">The type containing the method to search for.</param>
    /// <param name="methodName">The name of the type's method.</param>
    /// <param name="paramArray">The parameters for the method.</param>
    /// <returns>Information about the matched method.</returns>
    private static MethodInfo? FindMatchingMethod(Type objectType, string methodName, object[] paramArray)
    {
        return objectType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                         .FirstOrDefault(method => method.Name == methodName && (paramArray is null || MethodMatchesParameters(method, paramArray)));
    }

    /// <summary>
    /// Determine if a method's parameters match a list of user-defined parameters.
    /// </summary>
    /// <param name="method">The method to examine.</param>
    /// <param name="paramArray">The list of parameters that the method should have.</param>
    /// <returns>True of the list of expected parameters matches the method's parameters.</returns>
    private static bool MethodMatchesParameters(MethodBase method, object[] paramArray)
    {
        var parameters = method.GetParameters();
        if (parameters.Length != paramArray.Length)
        {
            return false;
        }

        for (var i = 0; i < paramArray.Length; ++i)
        {
            // We assume that a null object is of the expected type since we cannot check that
            if (paramArray[i] is not null && !parameters[i].ParameterType.IsInstanceOfType(paramArray[i]))
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
    private static object? InitializeInstance(Type objectType, object[] parameters)
    {
        return objectType.GetConstructors()
                         .FirstOrDefault(constructor => MethodMatchesParameters(constructor, parameters))?
                         .Invoke(parameters);
    }

    /// <summary>
    /// Log a message.
    /// </summary>
    /// <param name="message">The information to log.</param>
    private static void Log(string message)
    {
        Debug.WriteLine(message);
    }

    /// <summary>
    /// Send a exception message to the host and then throw the exception in the current application.
    /// </summary>
    /// <param name="notifier">Communication helper to send messages to the host application.</param>
    /// <param name="e">The exception that occurred.</param>
    private static void Log(NotificationHelper notifier, Exception e)
    {
        notifier.Log(e.Message, LogLevel.Error);
        throw e;
    }
}
