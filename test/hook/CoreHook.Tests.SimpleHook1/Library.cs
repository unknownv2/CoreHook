using System;

namespace CoreHook.Tests.SimpleHook1
{
    public class Library : IEntryPoint
    {

        public Library(object context, string arg1)
        {

        }

        public void Run(object context, string message)
        {
            Console.WriteLine(message);
        }
    }
}
