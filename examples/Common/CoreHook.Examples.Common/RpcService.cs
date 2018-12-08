using System;
using System.Threading;
using System.Threading.Tasks;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;
using CoreHook.IPC.Transport;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;

namespace CoreHook.Examples.Common
{
    public class RpcService
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

        public RpcService(ISessionFeature session, Type service, Func<RequestContext, Func<Task>, Task> handler)
        {
            _session = session;
            _service = service;
            _handler = handler;
        }

        public static RpcService CreateRpcService(
            string namedPipeName,
            IPipePlatform pipePlatform,
            ISessionFeature session,
            Type rpcService,
            Func<RequestContext, Func<Task>, Task> handler)
        {
            var service = new RpcService(session, rpcService, handler);

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

        public void HandleTransportConnection(ITransportChannel channel)
        {
            Console.WriteLine($"Connection received from pipe {_pipeName}.");

            var serverStream = channel.Connection.Stream;

            IJsonRpcServiceHost host = BuildServiceHost(_service);

            var serverHandler = new StreamRpcServerHandler(host);

            serverHandler.DefaultFeatures.Set(_session);

            using (var reader = new ByLineTextMessageReader(serverStream))
            using (var writer = new ByLineTextMessageWriter(serverStream))
            using (serverHandler.Attach(reader, writer))
            {
                // Wait for exit
                _session.CancellationToken.WaitHandle.WaitOne();
            }
        }
    }
}
