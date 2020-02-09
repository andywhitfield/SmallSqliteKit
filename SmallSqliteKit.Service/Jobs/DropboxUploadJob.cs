using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmallSqliteKit.Service.Jobs
{
    public class DropboxUploadJob : BackgroundServiceJob
    {
        public DropboxUploadJob(IServiceProvider serviceProvider, ILogger<DropboxUploadJob> logger) : base(serviceProvider, logger)
        {
        }

        protected override Task RunJobAsync(IServiceScope serviceScope)
        {
            _logger.LogInformation("TODO");
            return Task.CompletedTask;
        }
    }
}