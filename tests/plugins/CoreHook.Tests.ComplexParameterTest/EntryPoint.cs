using System;
using CoreHook.Tests.Plugins.Shared;

namespace CoreHook.Tests.ComplexParameterTest;

public class EntryPoint : IEntryPoint
{
    public EntryPoint(IContext context, ComplexParameter arg1) { }

    public void Run(IContext context, ComplexParameter complexParameter)
    {
        Console.WriteLine(complexParameter.Message);
        Console.WriteLine(complexParameter.HostProcessId);
    }
}
