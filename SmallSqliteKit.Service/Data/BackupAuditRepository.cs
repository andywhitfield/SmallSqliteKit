using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data
{
    public class BackupAuditRepository : IBackupAuditRepository
    {
        private const int MaxAuditEvents = 100;
        private readonly SqliteDataContext _context;

        public BackupAuditRepository(SqliteDataContext context)
        {
            _context = context;
        }

        public async Task AuditEventAsync(DatabaseBackup databaseBackup, string auditLog)
        {
            await _context.BackupAudits.AddAsync(new BackupAudit
            {
                DatabaseBackup = databaseBackup,
                AuditLog = auditLog,
                TimestampCreated = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public Task<List<BackupAudit>> GetAuditEventsAsync(int count)
        {
            if (count > MaxAuditEvents)
                throw new ArgumentOutOfRangeException(nameof(count), count, $"Can only get up to {MaxAuditEvents} number of audit events");
            return _context.BackupAudits.OrderByDescending(a => a.BackupAuditId).Take(count).ToListAsync();
        }
    }
}