using System;

namespace CoreHook.Tests.SimpleParameterTest;

public class EntryPoint : IEntryPoint
{
    public EntryPoint(IContext context, string arg1) { }

    public void Run(IContext context, string message) => Console.WriteLine(message);
}
