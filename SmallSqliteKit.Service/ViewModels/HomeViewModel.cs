using System.Collections.Generic;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.ViewModels
{
    public class HomeViewModel
    {
        public string BackupPath { get; set; }
        public bool IsLinkedToDropbox { get; set; }
        public IEnumerable<DatabaseBackup> DatabaseBackups { get; set; } = new List<DatabaseBackup>();
        public DatabaseBackup NewDatabaseModel => new DatabaseBackup();
        public IEnumerable<BackupAudit> AuditEvents { get; set; } = new List<BackupAudit>();
        public int? EditingBackupId { get; set; }
    }
}