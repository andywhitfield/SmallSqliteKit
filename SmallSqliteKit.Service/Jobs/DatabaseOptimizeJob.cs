using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Jobs;

public class DatabaseOptimizeJob : BackgroundServiceJob
{
    public DatabaseOptimizeJob(IServiceProvider serviceProvider, ILogger<DatabaseOptimizeJob> logger) : base(serviceProvider, logger)
    {
    }

    protected override async Task RunJobAsync(IServiceScope serviceScope)
    {
        var configRepository = serviceScope.ServiceProvider.GetRequiredService<IConfigRepository>();
        var backupAuditRepository = serviceScope.ServiceProvider.GetRequiredService<IBackupAuditRepository>();

        var databaseBackupRepository = serviceScope.ServiceProvider.GetRequiredService<IDatabaseBackupRepository>();

        foreach (var dbBackup in (await databaseBackupRepository.GetAllAsync()).Where(db => db.Optimize && db.OptimizeFrequency.HasValue))
        {
            var dbBackupDue = dbBackup.OptimizeFrequency.Value.NextDateTime(dbBackup.LastOptimizeTime);
            if (dbBackupDue < DateTime.UtcNow)
            {
                _logger.LogInformation($"Optimizing database: {dbBackup.DatabasePath} (last optimized: {dbBackup.LastOptimizeTime}; freq: {dbBackup.OptimizeFrequency})");
                try
                {
                    await OptimizeDbAsync(databaseBackupRepository, dbBackup, backupAuditRepository);
                    await backupAuditRepository.AuditEventAsync(dbBackup, "Optimize DB success");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Could not optimize db {dbBackup.DatabasePath}");
                    await backupAuditRepository.AuditEventAsync(dbBackup, $"Optimize DB failed: unexpected error: {ex.Message}");
                }
            }
            else
            {
                _logger.LogDebug($"Database optimize not due: {dbBackup.DatabasePath}, next optimize: {dbBackupDue} (last optimize: {dbBackup.LastOptimizeTime}; freq: {dbBackup.OptimizeFrequency})");
            }
        }
    }

    private async Task OptimizeDbAsync(IDatabaseBackupRepository databaseBackupRepository, DatabaseBackup dbBackup, IBackupAuditRepository backupAuditRepository)
    {
        await using SQLiteConnection dbToBackupConn = new($"Data Source={dbBackup.DatabasePath};FailIfMissing=True;");
        await dbToBackupConn.OpenAsync();
        await using var dbCommand = dbToBackupConn.CreateCommand();
        dbCommand.CommandType = CommandType.Text;
        dbCommand.CommandText = "PRAGMA optimize";
        await dbCommand.ExecuteNonQueryAsync();

        _logger.LogInformation($"Successfully optimized database: {dbBackup.DatabasePath}");
        dbBackup.LastOptimizeTime = DateTime.UtcNow;
        await databaseBackupRepository.UpdateAsync(dbBackup);
    }
}