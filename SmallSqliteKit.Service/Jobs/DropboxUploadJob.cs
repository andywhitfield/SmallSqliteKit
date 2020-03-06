using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;
using SmallSqliteKit.Service.Services;

namespace SmallSqliteKit.Service.Jobs
{
    public class DropboxUploadJob : BackgroundServiceJob
    {
        public DropboxUploadJob(IServiceProvider serviceProvider, ILogger<DropboxUploadJob> logger) : base(serviceProvider, logger)
        {
        }

        protected override async Task RunJobAsync(IServiceScope serviceScope)
        {
            var configRepository = serviceScope.ServiceProvider.GetRequiredService<IConfigRepository>();
            var backupAuditRepository = serviceScope.ServiceProvider.GetRequiredService<IBackupAuditRepository>();
            if (string.IsNullOrEmpty(await configRepository.GetDropboxTokenAsync()))
            {
                _logger.LogInformation("No dropbox token available, cannot continue with the upload job");
                return;
            }

            var backupPath = await configRepository.GetBackupPathAsync();
            var databaseBackupRepository = serviceScope.ServiceProvider.GetRequiredService<IDatabaseBackupRepository>();
            foreach (var dbBackup in (await databaseBackupRepository.GetAllAsync()).Where(db => db.UploadToDropbox && db.UploadToDropboxFrequency.HasValue && db.LastBackupTime.HasValue))
            {
                var uploadDue = dbBackup.UploadToDropboxFrequency.Value.NextDateTime(dbBackup.LastUploadToDropboxTime);
                if (uploadDue < DateTime.UtcNow)
                {
                    _logger.LogInformation($"Uploading last backup of database: {dbBackup.DatabasePath} (last upload: {dbBackup.LastUploadToDropboxTime}; freq: {dbBackup.UploadToDropboxFrequency})");
                    try
                    {
                        await UploadLastBackupAsync(databaseBackupRepository, dbBackup, backupPath, serviceScope.ServiceProvider.GetRequiredService<IDropboxUploadClient>());
                        await backupAuditRepository.AuditEventAsync(dbBackup, "Upload success");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Could not upload last backup of db {dbBackup.DatabasePath}");
                        await backupAuditRepository.AuditEventAsync(dbBackup, $"Upload failed: unexpected error: {ex.Message}");
                    }
                }
                else
                {
                    _logger.LogDebug($"Database upload not due: {dbBackup.DatabasePath}, next upload: {uploadDue} (last upload: {dbBackup.LastUploadToDropboxTime}; freq: {dbBackup.UploadToDropboxFrequency})");
                }
            }
        }

        private async Task UploadLastBackupAsync(IDatabaseBackupRepository databaseBackupRepository, DatabaseBackup dbBackup, string backupPath, IDropboxUploadClient dropboxUploadClient)
        {
            var backupFile = new FileInfo(DatabaseBackupJob.GetBackupFilename(dbBackup, backupPath, dbBackup.LastBackupTime.GetValueOrDefault()));
            if (!backupFile.Exists)
            {
                _logger.LogWarning($"Database backup file [{backupFile.FullName}] does not exist, cannot perform upload");
                return;
            }

            _logger.LogInformation($"Uploading latest database backup [{dbBackup.DatabasePath}]: {backupFile}");
            await dropboxUploadClient.UploadFileAsync(backupFile, $"{Path.GetFileName(dbBackup.DatabasePath)}.{dbBackup.DatabaseBackupId}");
            _logger.LogInformation($"Successfully uploaded latest database backup: {dbBackup.DatabasePath}");

            dbBackup.LastUploadToDropboxTime = DateTime.UtcNow;
            await databaseBackupRepository.UpdateAsync(dbBackup);
        }
    }
}