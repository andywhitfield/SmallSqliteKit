using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
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
            var configRepository = serviceScope.ServiceProvider.GetRequiredService<IConfigRepository>();
            var backupPath = await configRepository.GetBackupPathAsync();

            var databaseBackupRepository = serviceScope.ServiceProvider.GetRequiredService<IDatabaseBackupRepository>();

            foreach (var dbBackup in (await databaseBackupRepository.GetAllAsync()))
            {
                var dbBackupDue = dbBackup.BackupFrequency.NextDateTime(dbBackup.LastBackupTime);
                if (dbBackupDue < DateTime.UtcNow)
                {
                    _logger.LogInformation($"Backing up database: {dbBackup.DatabasePath} (last backup: {dbBackup.LastBackupTime}; freq: {dbBackup.BackupFrequency})");
                    try
                    {
                        await BackupDbAsync(databaseBackupRepository, dbBackup, backupPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Could not backup db {dbBackup.DatabasePath}");
                    }
                }
                else
                {
                    _logger.LogDebug($"Database not due to backup: {dbBackup.DatabasePath}, next backup: {dbBackupDue} (last backup: {dbBackup.LastBackupTime}; freq: {dbBackup.BackupFrequency})");
                }
            }
        }

        private async Task BackupDbAsync(IDatabaseBackupRepository databaseBackupRepository, DatabaseBackup dbBackup, string backupPath)
        {
            if (!File.Exists(dbBackup.DatabasePath))
            {
                _logger.LogWarning($"Database file [{dbBackup.DatabasePath}] does not exist, cannot perform backup");
                return;
            }

            var backupTime = DateTime.UtcNow;
            var backupFilename = GetBackupFilename(dbBackup, backupPath, backupTime);
            if (File.Exists(backupFilename))
            {
                _logger.LogError($"Backup file named [{backupFilename}] already exists, cannot perform backup");
                return;
            }

            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            using var dbToBackupConn = new SQLiteConnection($"Data Source={dbBackup.DatabasePath};FailIfMissing=True;");
            await dbToBackupConn.OpenAsync();
            using var dbCommand = dbToBackupConn.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = $"vacuum into @vacuumInto";
            var dbParam = dbCommand.CreateParameter();
            dbParam.ParameterName = "@vacuumInto";
            dbParam.Value = backupFilename;
            dbCommand.Parameters.Add(dbParam);
            await dbCommand.ExecuteNonQueryAsync();

            _logger.LogInformation($"Successfully backed up database: {dbBackup.DatabasePath}");
            dbBackup.LastBackupTime = backupTime;
            await databaseBackupRepository.UpdateAsync(dbBackup);
        }

        internal static string GetBackupFilename(DatabaseBackup dbBackup, string backupPath, DateTime backupTime)
        {
            var dbFilename = Path.GetFileNameWithoutExtension(dbBackup.DatabasePath);
            var dbExt = Path.GetExtension(dbBackup.DatabasePath);
            if (string.IsNullOrEmpty(dbExt))
                dbExt = ".db";
            return Path.Join(backupPath, $"{dbFilename}.backup.{backupTime:yyyyMMddHHmmss}{dbExt}");
        }
    }
}