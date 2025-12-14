using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.ViewModels;

public class HomeViewModel
{
    public string? BackupPath { get; set; }
    public bool IsLinkedToDropbox { get; set; }
    public IEnumerable<DatabaseBackup> DatabaseBackups { get; set; } = [];
    public DatabaseBackup NewDatabaseModel => new();
    public IEnumerable<BackupAudit> AuditEvents { get; set; } = [];
    public int? EditingBackupId { get; set; }
}