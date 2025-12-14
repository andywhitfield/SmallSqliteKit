using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;
using SmallSqliteKit.Service.Services;

namespace SmallSqliteKit.Service.Jobs;

public class DropboxUploadJob(IServiceProvider serviceProvider, ILogger<DropboxUploadJob> logger)
    : BackgroundServiceJob(serviceProvider, logger)
{
    protected override async Task RunJobAsync(IServiceScope serviceScope)
    {
        var configRepository = serviceScope.ServiceProvider.GetRequiredService<IConfigRepository>();
        var backupAuditRepository = serviceScope.ServiceProvider.GetRequiredService<IBackupAuditRepository>();
        var (dropboxAccessToken, dropboxRefreshToken) = await configRepository.GetDropboxTokensAsync();
        if (string.IsNullOrEmpty(dropboxAccessToken) || string.IsNullOrEmpty(dropboxRefreshToken))
        {
            _logger.LogInformation("No dropbox tokens available, cannot continue with the upload job");
            return;
        }

        var backupPath = await configRepository.GetBackupPathAsync();
        var databaseBackupRepository = serviceScope.ServiceProvider.GetRequiredService<IDatabaseBackupRepository>();
        foreach (var dbBackup in (await databaseBackupRepository.GetAllAsync()).Where(db => db.UploadToDropbox && db.UploadToDropboxFrequency.HasValue && db.LastBackupTime.HasValue))
        {
            var uploadDue = (dbBackup.UploadToDropboxFrequency ?? BackupFrequency.Monthly).NextDateTime(dbBackup.LastUploadToDropboxTime);
            if (uploadDue < DateTime.UtcNow)
            {
                _logger.LogInformation("Uploading last backup of database: {DatabasePath} (last upload: {LastUploadToDropboxTime}; freq: {UploadToDropboxFrequency})", dbBackup.DatabasePath, dbBackup.LastUploadToDropboxTime, dbBackup.UploadToDropboxFrequency);
                try
                {
                    await UploadLastBackupAsync(databaseBackupRepository, dbBackup, backupPath, serviceScope.ServiceProvider.GetRequiredService<IDropboxUploadClient>());
                    await backupAuditRepository.AuditEventAsync(dbBackup, "Dropbox Upload success");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not upload last backup of db {DatabasePath}", dbBackup.DatabasePath);
                    await backupAuditRepository.AuditEventAsync(dbBackup, $"Upload failed: unexpected error: {ex.Message}");
                }
            }
            else
            {
                _logger.LogDebug("Database upload not due: {DatabasePath}, next upload: {UploadDue} (last upload: {LastUploadToDropboxTime}; freq: {UploadToDropboxFrequency})", dbBackup.DatabasePath, uploadDue, dbBackup.LastUploadToDropboxTime, dbBackup.UploadToDropboxFrequency);
            }
        }
    }

    private async Task UploadLastBackupAsync(IDatabaseBackupRepository databaseBackupRepository, DatabaseBackup dbBackup, string backupPath, IDropboxUploadClient dropboxUploadClient)
    {
        var backupFile = new FileInfo(DatabaseBackupJob.GetBackupFilename(dbBackup, backupPath, dbBackup.LastBackupTime.GetValueOrDefault()));
        if (!backupFile.Exists)
        {
            _logger.LogWarning("Database backup file [{FullName}] does not exist, cannot perform upload", backupFile.FullName);
            return;
        }

        _logger.LogInformation("Uploading latest database backup [{DatabasePath}]: {BackupFile}", dbBackup.DatabasePath, backupFile);
        await dropboxUploadClient.UploadFileAsync(backupFile, $"{Path.GetFileName(dbBackup.DatabasePath)}.{dbBackup.DatabaseBackupId}");
        _logger.LogInformation("Successfully uploaded latest database backup: {DatabasePath}", dbBackup.DatabasePath);

        dbBackup.LastUploadToDropboxTime = DateTime.UtcNow;
        await databaseBackupRepository.UpdateAsync(dbBackup);
    }
}