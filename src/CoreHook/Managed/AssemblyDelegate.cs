using System.Reflection;
using System.Runtime.InteropServices;

namespace CoreHook.Managed;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct AssemblyDelegate
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public readonly string AssemblyPath;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public readonly string TypeNameQualified;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public readonly string MethodName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public readonly string? DelegateTypeName;

    public AssemblyDelegate(string assemblyName, string typeName, string methodName) : this()
    {
        var assembly = Assembly.Load(assemblyName);
        AssemblyPath = assembly.Location;
        TypeNameQualified = Assembly.CreateQualifiedName(assemblyName, typeName);
        MethodName = methodName;
    }

    public AssemblyDelegate(string assemblyName, string typeName, string methodName, string? delegateTypeName) : this(assemblyName, typeName, methodName)
    {
        DelegateTypeName = delegateTypeName;
    }
}
