using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Jobs
{
    public class DatabaseBackupJob : BackgroundServiceJob
    {
        public DatabaseBackupJob(IServiceProvider serviceProvider, ILogger<DatabaseBackupJob> logger) : base(serviceProvider, logger)
        {
        }

        protected override async Task RunJobAsync(IServiceScope serviceScope)
        {
            var databaseBackupRepository = serviceScope.ServiceProvider.GetRequiredService<IDatabaseBackupRepository>();
            foreach (var dbBackup in (await databaseBackupRepository.GetAllAsync()))
            {
                var dbBackupDue = dbBackup.BackupFrequency.NextDateTime(dbBackup.LastBackupTime);
                if (dbBackupDue < DateTime.UtcNow)
                {
                    _logger.LogInformation($"Backing up database: {dbBackup.DatabasePath} (last backup: {dbBackup.LastBackupTime}; freq: {dbBackup.BackupFrequency})");
                    await BackupDbAsync(databaseBackupRepository, dbBackup);
                }
                else
                {
                    _logger.LogDebug($"Database not due to backup: {dbBackup.DatabasePath}, next backup: {dbBackupDue} (last backup: {dbBackup.LastBackupTime}; freq: {dbBackup.BackupFrequency})");
                }
            }
        }

        private async Task BackupDbAsync(IDatabaseBackupRepository databaseBackupRepository, DatabaseBackup dbBackup)
        {
            // TODO: do the backup
            _logger.LogInformation($"Successfully backed up database: {dbBackup.DatabasePath}");
            dbBackup.LastBackupTime = DateTime.UtcNow;
            await databaseBackupRepository.UpdateAsync(dbBackup);
        }
    }
}