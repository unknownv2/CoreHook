using System;
using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;

namespace CoreHook.UWP.FileMonitor.Shared
{
    public interface IFileMonitor
    {
        [JsonRpcMethod]
        Task OnCreateFile(string[] fileNames);
    }
}
