using System.Collections.Generic;
using System.Threading.Tasks;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data
{
    public interface IDatabaseBackupRepository
    {
        Task<List<DatabaseBackup>> GetAllAsync();
        Task<DatabaseBackup> GetAsync(int databaseBackupId);
        Task AddAsync(string databasePath, BackupFrequency backupFrequency, bool uploadToDropbox, BackupFrequency? uploadToDropboxFrequency);
        Task DeleteAsync(DatabaseBackup databaseBackup);
        Task UpdateAsync(DatabaseBackup dbBackup);
    }
}