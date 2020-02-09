using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data
{
    public class DatabaseBackupRepository : IDatabaseBackupRepository
    {
        private readonly SqliteDataContext _context;

        public DatabaseBackupRepository(SqliteDataContext context)
        {
            _context = context;
        }

        public Task<List<DatabaseBackup>> GetAllAsync()
        {
            return _context.DatabaseBackups.ToListAsync();
        }
        
        public Task<DatabaseBackup> GetAsync(int databaseBackupId)
        {
            return _context.DatabaseBackups.FindAsync(databaseBackupId).AsTask();
        }

        public async Task AddAsync(string databasePath, BackupFrequency backupFrequency, bool uploadToDropbox, BackupFrequency? uploadToDropboxFrequency)
        {
            await _context.DatabaseBackups.AddAsync(new DatabaseBackup
            {
                DatabasePath = databasePath,
                BackupFrequency = backupFrequency,
                UploadToDropbox = uploadToDropbox,
                UploadToDropboxFrequency = uploadToDropbox ? uploadToDropboxFrequency : null
            });
            await _context.SaveChangesAsync();
        }

        public Task DeleteAsync(DatabaseBackup databaseBackup)
        {
            _context.DatabaseBackups.Remove(databaseBackup);
            return _context.SaveChangesAsync();
        }

        public Task UpdateAsync(DatabaseBackup databaseBackup)
        {
            _context.DatabaseBackups.Update(databaseBackup);
            return _context.SaveChangesAsync();
        }
    }
}