using System;
using System.Collections.Generic;
using System.Text;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;
using System.IO;

namespace CoreHook.UWP.FileMonitor
{
    internal class Logger
    {
        internal static NamedPipeServer CreateServer(string namedPipeName, IPipePlatform pipePlatform)
        {
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, HandleRequest, ReadRequest);
        }
        internal static NamedPipeServer CreateStringServer(string namedPipeName, IPipePlatform pipePlatform)
        {
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, HandleRequest, ReadStringRequest);
        }
        private static string ReadRequest(StreamReader reader)
        {
            var brReader = new BinaryReader(reader.BaseStream);

            var nBytes = brReader.ReadUInt16();
            var nFacility = brReader.ReadByte();
            var nSeverity = brReader.ReadByte();
            var nProcessId = brReader.ReadInt32();
            var ftOccurance = brReader.ReadInt64();
            var fTerminate = brReader.ReadInt32();
            var message = Encoding.UTF8.GetString(brReader.ReadBytes(nBytes - 20));
            return message;
        }
        private static string ReadStringRequest(StreamReader reader)
        {
            var message = reader.ReadLine();
            //Log(message);
            return message;
        }
        private static void HandleRequest(string request, NamedPipeServer.Connection connection)
        {
            if(!string.IsNullOrEmpty(request))
            {
                Log(request);
            }
        }
        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
