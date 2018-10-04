using System;

namespace CoreHook.Tests.SimpleHook1
{
    public class Library : IEntryPoint
    {
        public Library(IContext context, string arg1)
        {
        }

        public void Run(IContext context, string message)
        {
            Console.WriteLine(message);
        }
    }
}
