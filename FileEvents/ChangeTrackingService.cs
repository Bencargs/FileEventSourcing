using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace FileEvents
{
    public class ChangeTrackingService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ISourceControl _sourceControl;

        public ChangeTrackingService(
            IConfiguration configuration,
            ISourceControl sourceControl)
        {
            _configuration = configuration;
            _sourceControl = sourceControl;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var repositories = _configuration.GetSection("Repositories").Get<string[]>();
            foreach (var repo in repositories)
                await _sourceControl.Add(repo);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
