using Microsoft.EntityFrameworkCore;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data;

public class DatabaseBackupRepository(SqliteDataContext context) : IDatabaseBackupRepository
{
    public Task<List<DatabaseBackup>> GetAllAsync() => context.DatabaseBackups.ToListAsync();
    
    public Task<DatabaseBackup?> GetAsync(int databaseBackupId) => context.DatabaseBackups.FindAsync(databaseBackupId).AsTask();

    public async Task AddAsync(string databasePath, BackupFrequency backupFrequency, bool uploadToDropbox, BackupFrequency? uploadToDropboxFrequency)
    {
        await context.DatabaseBackups.AddAsync(new DatabaseBackup
        {
            DatabasePath = databasePath,
            BackupFrequency = backupFrequency,
            UploadToDropbox = uploadToDropbox,
            UploadToDropboxFrequency = uploadToDropbox ? uploadToDropboxFrequency : null
        });
        await context.SaveChangesAsync();
    }

    public Task DeleteAsync(DatabaseBackup databaseBackup)
    {
        context.DatabaseBackups.Remove(databaseBackup);
        return context.SaveChangesAsync();
    }

    public Task UpdateAsync(DatabaseBackup databaseBackup)
    {
        context.DatabaseBackups.Update(databaseBackup);
        return context.SaveChangesAsync();
    }
}