using System.Collections.Generic;
using System.IO;

namespace SmallSqliteKit.Service.Services
{
    public interface IBackupFilePurger
    {
        IEnumerable<string> PurgeBackups(DirectoryInfo backupPath, int backupsToKeep, string filename = null);
    }
}