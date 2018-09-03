using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;

namespace CoreHook.FileMonitor.Shared
{
    public interface IFileMonitor
    {
        [JsonRpcMethod]
        Task OnCreateFile(string[] fileNames);
    }
}
