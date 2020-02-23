using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SmallSqliteKit.Service.Services
{
    public class BackupFilePurger : IBackupFilePurger
    {
        private readonly ILogger<BackupFilePurger> _logger;

        public BackupFilePurger(ILogger<BackupFilePurger> logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> PurgeBackups(DirectoryInfo backupPath, int backupsToKeep, string filename = null) => PurgeBackupsInternal(backupPath, backupsToKeep, filename).ToList();

        public IEnumerable<string> PurgeBackupsInternal(DirectoryInfo backupPath, int backupsToKeep, string filename)
        {
            if (!backupPath?.Exists ?? true)
                yield break;

            const string backupFileSegment = ".backup.";
            foreach (var backup in backupPath
                .GetFiles($"*{backupFileSegment}*")
                .Select(fi =>
                {
                    var nullValue = new { FileInfo = (FileInfo)null, BackupName = (string)null, BackupDate = (DateTime?)null };
                    var backupIdx = fi.Name.LastIndexOf(backupFileSegment);
                    if (backupIdx < 0)
                        return nullValue;

                    var filenameWithoutExt = fi.Name.Substring(0, backupIdx);
                    if (!string.IsNullOrEmpty(filename) && (filenameWithoutExt != Path.GetFileNameWithoutExtension(filename) || fi.Extension != Path.GetExtension(filename)))
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

                    return new { FileInfo = fi, BackupName = filenameWithoutExt, BackupDate = (DateTime?)backupDateTime };
                })
                .Where(backups => backups.FileInfo != null)
                .GroupBy(backups => backups.BackupName))
            {
                foreach (var backupToDelete in backup.OrderByDescending(b => b.BackupDate).Skip(backupsToKeep))
                {
                    _logger.LogInformation($"Removing old backup: {backupToDelete.FileInfo.FullName}");
                    backupToDelete.FileInfo.Delete();
                    yield return backupToDelete.FileInfo.FullName;
                }
            }
        }
    }
}