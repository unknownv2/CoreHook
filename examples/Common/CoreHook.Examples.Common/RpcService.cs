using System;
using System.Threading.Tasks;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;
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

            Task.Factory.StartNew(() => service.CreateServer(namedPipeName, pipePlatform),
                TaskCreationOptions.LongRunning);

            return service;
        }

        private static readonly IJsonRpcContractResolver myContractResolver = new JsonRpcContractResolver
        {
            // Use camelcase for RPC method names.
            NamingStrategy = new CamelCaseJsonRpcNamingStrategy(),
            // Use camelcase for the property names in parameter value objects
            ParameterValueConverter = new CamelCaseJsonValueConverter()
        };

        private INamedPipeServer CreateServer(string namedPipeName, IPipePlatform pipePlatform)
        {
            _pipeName = namedPipeName;
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, HandleConnection);
        }

        public IJsonRpcServiceHost BuildServiceHost(Type service)
        {
            var builder = new JsonRpcServiceHostBuilder
            {
                ContractResolver = myContractResolver,
            };

            builder.Register(service);

            builder.Intercept(_handler);

            return builder.Build();
        }

        public void HandleConnection(IPC.IConnection connection)
        {
            Console.WriteLine($"Connection received from pipe {_pipeName}");

            var pipeServer = connection.ServerStream;

            IJsonRpcServiceHost host = BuildServiceHost(_service);

            var serverHandler = new StreamRpcServerHandler(host);

            serverHandler.DefaultFeatures.Set(_session);

            using (var reader = new ByLineTextMessageReader(pipeServer))
            using (var writer = new ByLineTextMessageWriter(pipeServer))
            using (serverHandler.Attach(reader, writer))
            {
                // Wait for exit
                _session.CancellationToken.WaitHandle.WaitOne();
            }
        }
    }
}
