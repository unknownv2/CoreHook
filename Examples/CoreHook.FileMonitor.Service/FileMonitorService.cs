using System;
using System.Collections.Generic;
using System.Text;
using JsonRpc.Standard;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;

namespace CoreHook.FileMonitor.Service
{
    public class FileMonitorService : JsonRpcService
    {
        private FileMonitorSessionFeature Session => RequestContext.Features.Get<FileMonitorSessionFeature>();


        [JsonRpcMethod]
        public void OnCreateFile(string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                Console.WriteLine(fileName);
            }
        }
        [JsonRpcMethod]
        public void PrintStringList(string[] files)
        {
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
        }
    }
}
