using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Services;

namespace SmallSqliteKit.Service.Jobs
{
    public class PurgeJob : BackgroundServiceJob
    {
        private readonly IBackupFilePurger _backupFilePurger;

        public PurgeJob(IServiceProvider serviceProvider, ILogger<DropboxUploadJob> logger, IBackupFilePurger backupFilePurger) : base(serviceProvider, logger)
        {
            _backupFilePurger = backupFilePurger;
        }

        protected override async Task RunJobAsync(IServiceScope serviceScope)
        {
            var configRepository = serviceScope.ServiceProvider.GetRequiredService<IConfigRepository>();
            var backupPath = new DirectoryInfo(await configRepository.GetBackupPathAsync());
            var backupsToKeep = await configRepository.GetBackupFileCountAsync();

            _backupFilePurger.PurgeBackups(backupPath, backupsToKeep);
        }
    }
}