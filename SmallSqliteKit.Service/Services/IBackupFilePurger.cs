using System.Collections.Generic;
using System.IO;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Services
{
    public interface IBackupFilePurger
    {
        IEnumerable<string> PurgeBackups(DirectoryInfo backupPath, int backupsToKeep, DatabaseBackup backupToDelete = null);
    }
}