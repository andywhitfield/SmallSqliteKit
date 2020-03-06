using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Services
{
    public class BackupFilePurger : IBackupFilePurger
    {
        private readonly ILogger<BackupFilePurger> _logger;

        public BackupFilePurger(ILogger<BackupFilePurger> logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> PurgeBackups(DirectoryInfo backupPath, int backupsToKeep, DatabaseBackup backupToDelete = null) => PurgeBackupsInternal(backupPath, backupsToKeep, backupToDelete).ToList();

        private IEnumerable<string> PurgeBackupsInternal(DirectoryInfo backupPath, int backupsToKeep, DatabaseBackup backupToDelete)
        {
            if (!backupPath?.Exists ?? true)
                yield break;

            const string backupFileSegment = ".backup.";
            foreach (var backup in backupPath
                .GetFiles($"*{backupFileSegment}*")
                .Select(fi =>
                {
                    var nullValue = new { FileInfo = (FileInfo)null, BackupId = default(int), BackupDate = (DateTime?)null };
                    var backupIdx = fi.Name.LastIndexOf(backupFileSegment);
                    if (backupIdx < 0)
                        return nullValue;

                    var filenameAndIdWithoutExt = fi.Name.Substring(0, backupIdx);
                    var filenameAndIdIdx = filenameAndIdWithoutExt.LastIndexOf('.');
                    if (filenameAndIdIdx < 0)
                        return nullValue;
                    
                    var filenameWithoutExt = filenameAndIdWithoutExt.Substring(0, filenameAndIdIdx);
                    var backupIdSegment = filenameAndIdWithoutExt.Substring(filenameAndIdIdx + 1);
                    if (!int.TryParse(backupIdSegment, out var backupId))
                        return nullValue;

                    if (backupToDelete != null && (
                        backupId != backupToDelete.DatabaseBackupId ||
                        filenameWithoutExt != Path.GetFileNameWithoutExtension(backupToDelete.DatabasePath) ||
                        fi.Extension != Path.GetExtension(backupToDelete.DatabasePath)
                    ))
                        return nullValue;

                    var dateAndExt = fi.Name.Substring(backupIdx + backupFileSegment.Length);
                    var datePartIdx = dateAndExt.IndexOf('.');
                    if (datePartIdx < 0)
                        return nullValue;
                    if (dateAndExt.Substring(datePartIdx + 1).Contains('.'))
                        return nullValue;

                    var datePart = dateAndExt.Substring(0, datePartIdx);
                    if (!DateTime.TryParseExact(datePart, "yyyyMMddHHmmss", null, DateTimeStyles.AssumeUniversal, out var backupDateTime))
                        return nullValue;

                    _logger.LogTrace($"Backup file: [{fi.Name}]");

                    return new { FileInfo = fi, BackupId = backupId, BackupDate = (DateTime?)backupDateTime };
                })
                .Where(backups => backups.FileInfo != null)
                .GroupBy(backups => backups.BackupId))
            {
                foreach (var toDelete in backup.OrderByDescending(b => b.BackupDate).Skip(backupsToKeep))
                {
                    _logger.LogInformation($"Removing old backup: {toDelete.FileInfo.FullName}");
                    toDelete.FileInfo.Delete();
                    yield return toDelete.FileInfo.FullName;
                }
            }
        }
    }
}