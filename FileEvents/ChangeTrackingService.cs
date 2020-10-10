using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileEvents
{
    public class ChangeTrackingService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ISourceControl _sourceControl;
        private static ICommandHandler[] _commandHandlers;

        public ChangeTrackingService(
            IConfiguration configuration,
            ISourceControl sourceControl)
        {
            _configuration = configuration;
            _sourceControl = sourceControl;
            _commandHandlers = new ICommandHandler[]
            {
                new AddCommandHandler(sourceControl),
                new PreviewCommandHandler(sourceControl)
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var repositories = _configuration.GetSection("Repositories").Get<string[]>() ?? new string[0];
            var initialiseTasks = repositories.Select(x => _sourceControl.Add(x));
            await Task.WhenAll(initialiseTasks);
            await HandleMessages(cancellationToken);

            await StopAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task HandleMessages(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                using var server = new NamedPipeServerStream("FileEvents");
                await server.WaitForConnectionAsync(cancellation);
                using var stream = new StreamReader(server);
                while (server.IsConnected && !cancellation.IsCancellationRequested)
                {
                    var message = await stream.ReadToEndAsync();
                    if (string.IsNullOrWhiteSpace(message))
                        await Task.Delay(500);

                    await HandleCommand(message);
                }
            }
        }

        private Task HandleCommand(string message)
        {
            var command = JsonConvert.DeserializeObject<dynamic>(message);            
            var handlers = _commandHandlers.Where(x => x.CanHandle(command));
            var handleTasks = handlers.Select(x => (Task)x.Execute(command));
            
            return Task.WhenAll(handleTasks);
        }
    }
}
