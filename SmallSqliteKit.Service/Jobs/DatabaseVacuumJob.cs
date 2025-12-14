using System.Data;
using System.Data.SQLite;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Jobs;

public class DatabaseVacuumJob(IServiceProvider serviceProvider, ILogger<DatabaseVacuumJob> logger)
    : BackgroundServiceJob(serviceProvider, logger)
{
    protected override async Task RunJobAsync(IServiceScope serviceScope)
    {
        var configRepository = serviceScope.ServiceProvider.GetRequiredService<IConfigRepository>();
        var backupAuditRepository = serviceScope.ServiceProvider.GetRequiredService<IBackupAuditRepository>();

        var databaseBackupRepository = serviceScope.ServiceProvider.GetRequiredService<IDatabaseBackupRepository>();

        foreach (var dbBackup in (await databaseBackupRepository.GetAllAsync()).Where(db => db.Vacuum && db.VacuumFrequency.HasValue))
        {
            var dbBackupDue = (dbBackup.VacuumFrequency ?? BackupFrequency.Monthly).NextDateTime(dbBackup.LastVacuumTime);
            if (dbBackupDue < DateTime.UtcNow)
            {
                _logger.LogInformation("Vacuuming database: {DatabasePath} (last vacuum: {LastVacuumTime}; freq: {VacuumFrequency})", dbBackup.DatabasePath, dbBackup.LastVacuumTime, dbBackup.VacuumFrequency);
                try
                {
                    await VacuumDbAsync(databaseBackupRepository, dbBackup, backupAuditRepository);
                    await backupAuditRepository.AuditEventAsync(dbBackup, "Vacuum DB success");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not vacuum db {DatabasePath}", dbBackup.DatabasePath);
                    await backupAuditRepository.AuditEventAsync(dbBackup, $"Vacuum DB failed: unexpected error: {ex.Message}");
                }
            }
            else
            {
                _logger.LogDebug("Database vacuum not due: {DatabasePath}, next vacuum: {BackupDue} (last vacuum: {LastVacuumTime}; freq: {VacuumFrequency})", dbBackup.DatabasePath, dbBackupDue, dbBackup.LastVacuumTime, dbBackup.VacuumFrequency);
            }
        }
    }

    private async Task VacuumDbAsync(IDatabaseBackupRepository databaseBackupRepository, DatabaseBackup dbBackup, IBackupAuditRepository backupAuditRepository)
    {
        await using SQLiteConnection dbToBackupConn = new($"Data Source={dbBackup.DatabasePath};FailIfMissing=True;");
        await dbToBackupConn.OpenAsync();
        await using var dbCommand = dbToBackupConn.CreateCommand();
        dbCommand.CommandType = CommandType.Text;
        dbCommand.CommandText = "vacuum";
        await dbCommand.ExecuteNonQueryAsync();

        _logger.LogInformation("Successfully vacuumed database: {DatabasePath}", dbBackup.DatabasePath);
        dbBackup.LastVacuumTime = DateTime.UtcNow;
        await databaseBackupRepository.UpdateAsync(dbBackup);
    }
}