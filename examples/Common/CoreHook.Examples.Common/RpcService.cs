using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;

using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoreHook.Examples.Common;

public class RpcService<T>
{
    private readonly ISessionFeature _session;
    private readonly Type _service;
    private string _pipeName;
    private readonly Func<RequestContext, Func<Task>, Task> _handler;
    private static Thread _rpcServerThread;

    private static readonly IJsonRpcContractResolver MyContractResolver = new JsonRpcContractResolver
    {
        // Use camelcase for RPC method names.
        NamingStrategy = new CamelCaseJsonRpcNamingStrategy(),
        // Use camelcase for the property names in parameter value objects
        ParameterValueConverter = new CamelCaseJsonValueConverter()
    };

    public RpcService(ISessionFeature session, Func<RequestContext, Func<Task>, Task> handler)
    {
        _service = typeof(T);

        _session = session;
        _handler = handler;
    }

    public static RpcService<T> CreateRpcService(string namedPipeName, IPipePlatform pipePlatform, ISessionFeature session, Func<RequestContext, Func<Task>, Task> handler)
    {
        var service = new RpcService<T>(session, handler);

        _rpcServerThread = new Thread(() => service.CreateServer(namedPipeName, pipePlatform))
        {
            IsBackground = true
        };
        _rpcServerThread.Start();

        return service;
    }

    private INamedPipe CreateServer(string namedPipeName, IPipePlatform pipePlatform)
    {
        _pipeName = namedPipeName;
        //TODO: ensure the pipe is disposed when detaching the service (not supported yet)
        return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, HandleTransportConnection);
    }

    public IJsonRpcServiceHost BuildServiceHost(Type service)
    {
        var builder = new JsonRpcServiceHostBuilder
        {
            ContractResolver = MyContractResolver,
        };

        builder.Register(service);

        builder.Intercept(_handler);

        return builder.Build();
    }

    public void HandleTransportConnection(INamedPipe channel)
    {
        Console.WriteLine($"Connection received from pipe {_pipeName}.");

        IJsonRpcServiceHost host = BuildServiceHost(_service);

        var serverHandler = new StreamRpcServerHandler(host);

        serverHandler.DefaultFeatures.Set(_session);

        using (var reader = new ByLineTextMessageReader(channel.Stream))
        using (var writer = new ByLineTextMessageWriter(channel.Stream))
        using (serverHandler.Attach(reader, writer))
        {
            // Wait for exit
            _session.CancellationToken.WaitHandle.WaitOne();
        }
    }
}
