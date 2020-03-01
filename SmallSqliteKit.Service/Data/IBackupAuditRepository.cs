using System.Collections.Generic;
using System.Threading.Tasks;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data
{
    public interface IBackupAuditRepository
    {
         Task AuditEventAsync(DatabaseBackup databaseBackup, string auditLog);
         Task<List<BackupAudit>> GetAuditEventsAsync(int count);
    }
}