using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using SmallSqliteKit.Service.Models;
using SmallSqliteKit.Service.Services;
using Xunit;
using Xunit.Abstractions;

namespace SmallSqliteKit.Service.Tests.Services
{
    public class BackupFilePurgerTest : IDisposable
    {
        private DirectoryInfo _tempDir;
        private readonly XunitLogger _xunitLogger;

        public BackupFilePurgerTest(ITestOutputHelper testOutputHelper) => _xunitLogger = new XunitLogger(testOutputHelper);

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("/a/path/that/does/not/exist")]
        public void Given_NonExistentPath_Should_RemoveNoFiles(string backupPath)
        {
            var purger = new BackupFilePurger(_xunitLogger.CreateLogger<BackupFilePurger>());
            Assert.Empty(purger.PurgeBackups(backupPath == null ? null : new DirectoryInfo(backupPath), 0));
        }

        [Fact]
        public void Given_OneFileMatch_Should_RemoveFile_When_BackupCountIsZero()
        {
            var backupFiles = new[] { $"file.1.backup.{DateTime.UtcNow:yyyyMMddHHmmss}.db" };
            var (purger, backupPath) = CreateBackupFilePurger(backupFiles);
            Assert.Empty(purger.PurgeBackups(backupPath, 5));
            Assert.Equal(backupFiles, backupPath.GetFilenames());

            Assert.Empty(purger.PurgeBackups(backupPath, 1));
            Assert.Equal(backupFiles, backupPath.GetFilenames());

            Assert.Equal(backupFiles, purger.PurgeBackups(backupPath, 0).ToFilenames());
            Assert.Equal(new string[0], backupPath.GetFilenames());
        }

        [Fact]
        public void Given_ThreeFileMatches_Should_RemoveByOldestFileFirst()
        {
            var backupFiles = new[] {
                $"file.1.backup.{DateTime.UtcNow.AddHours(-1):yyyyMMddHHmmss}.db",
                $"file.1.backup.{DateTime.UtcNow:yyyyMMddHHmmss}.db",
                $"file.1.backup.{DateTime.UtcNow.AddHours(-2):yyyyMMddHHmmss}.db"
            };

            var (purger, backupPath) = CreateBackupFilePurger(backupFiles);
            Assert.Empty(purger.PurgeBackups(backupPath, 5));
            backupPath.GetFilenames().Should().HaveCount(backupFiles.Length).And.Contain(backupFiles);

            Assert.Empty(purger.PurgeBackups(backupPath, 3));
            backupPath.GetFilenames().Should().HaveCount(backupFiles.Length).And.Contain(backupFiles);

            purger.PurgeBackups(backupPath, 2).ToFilenames().Should().HaveCount(1).And.Contain(backupFiles[2]);
            backupPath.GetFilenames().Should().HaveCount(2).And.Contain(backupFiles[0]).And.Contain(backupFiles[1]);

            purger.PurgeBackups(backupPath, 1).ToFilenames().Should().HaveCount(1).And.Contain(backupFiles[0]);
            backupPath.GetFilenames().Should().HaveCount(1).And.Contain(backupFiles[1]);

            purger.PurgeBackups(backupPath, 0).ToFilenames().Should().HaveCount(1).And.Contain(backupFiles[1]);
            Assert.Empty(backupPath.GetFilenames());
        }

        [Fact]
        public void Given_MultipleFileMatches_Should_RemoveByOldestFileFirstForEach()
        {
            var backupFiles = new[] {
                $"file.1.backup.{DateTime.UtcNow.AddHours(-1):yyyyMMddHHmmss}.db",
                $"file.1.backup.{DateTime.UtcNow:yyyyMMddHHmmss}.db",
                $"file.1.backup.{DateTime.UtcNow.AddHours(-2):yyyyMMddHHmmss}.db",
                $"file.2.backup.{DateTime.UtcNow.AddHours(-4):yyyyMMddHHmmss}.db",
                $"file.3.backup.{DateTime.UtcNow.AddHours(-3):yyyyMMddHHmmss}.db",
                $"file.3.backup.{DateTime.UtcNow.AddHours(-1):yyyyMMddHHmmss}.db"
            };

            var (purger, backupPath) = CreateBackupFilePurger(backupFiles);
            Assert.Empty(purger.PurgeBackups(backupPath, 5));
            backupPath.GetFilenames().Should().HaveCount(backupFiles.Length).And.Contain(backupFiles);

            Assert.Empty(purger.PurgeBackups(backupPath, 3));
            backupPath.GetFilenames().Should().HaveCount(backupFiles.Length).And.Contain(backupFiles);

            purger.PurgeBackups(backupPath, 2).ToFilenames().Should().HaveCount(1).And.Contain(backupFiles[2]);
            backupPath.GetFilenames().Should().HaveCount(5).And.Contain(backupFiles.Except(new[] { backupFiles[2] }));

            purger.PurgeBackups(backupPath, 1).ToFilenames().Should().HaveCount(2).And.Contain(backupFiles[0]).And.Contain(backupFiles[4]);
            backupPath.GetFilenames().Should().HaveCount(3).And.Contain(new[] { backupFiles[1], backupFiles[3], backupFiles[5] });

            purger.PurgeBackups(backupPath, 0).ToFilenames().Should().HaveCount(3).And.Contain(new[] { backupFiles[1], backupFiles[3], backupFiles[5] });
            Assert.Empty(backupPath.GetFilenames());
        }

        [Fact]
        public void Given_FileNotMatchingExpectedFormat_Should_Ignore()
        {
            var backupFiles = new[] {
                $"file.1.backup.{DateTime.UtcNow.AddHours(-1):yyyyMMddHHmmss}.db",
                $"file.1.backup.{DateTime.UtcNow:yyyyMMddHHmmss}.db",
                $"file.1.backup.{DateTime.UtcNow:yyyyMMddHHmm}.db",
                $"file.1.backup.not-a-date.db",
                $"file.1.db",
                $"file.{DateTime.UtcNow:yyyyMMddHHmmss}.db",
                $"file.x.{DateTime.UtcNow:yyyyMMddHHmmss}.db",
                $"file.1.{DateTime.UtcNow:yyyyMMddHHmmss}.db",
                $"file.1.backup.{DateTime.UtcNow:yyyyMMddHHmmss}.db.save",
                $"file.1.backup.{DateTime.UtcNow.AddHours(-2):yyyyMMddHHmmss}.db"
            };

            var (purger, backupPath) = CreateBackupFilePurger(backupFiles);
            purger.PurgeBackups(backupPath, 1).ToFilenames().Should().HaveCount(2).And.Contain(backupFiles[0]).And.Contain(backupFiles[9]);
            backupPath.GetFilenames().Should().HaveCount(8).And.Contain(backupFiles.Except(new[] { backupFiles[0], backupFiles[9] }));
        }

        [Fact]
        public void Given_MultipleFiles_When_OnlyDeletingNamedFile_Should_LeaveOtherBackupsInPlace()
        {
            var backupFiles = new[] {
                $"file.1.backup.{DateTime.UtcNow.AddHours(-1):yyyyMMddHHmmss}.db",
                $"file.1.backup.{DateTime.UtcNow:yyyyMMddHHmmss}.db",
                $"file.1.backup.{DateTime.UtcNow.AddHours(-2):yyyyMMddHHmmss}.db",
                $"file.2.backup.{DateTime.UtcNow.AddHours(-4):yyyyMMddHHmmss}.db",
                $"file.3.backup.{DateTime.UtcNow.AddHours(-3):yyyyMMddHHmmss}.db",
                $"file.3.backup.{DateTime.UtcNow.AddHours(-1):yyyyMMddHHmmss}.db"
            };

            var (purger, backupPath) = CreateBackupFilePurger(backupFiles);
            purger.PurgeBackups(backupPath, 1, DbBackup(3, "file.db")).ToFilenames().Should().HaveCount(1).And.Contain(backupFiles[4]);
            backupPath.GetFilenames().Should().HaveCount(5).And.Contain(backupFiles.Except(new[] { backupFiles[4] }));

            Assert.Empty(purger.PurgeBackups(backupPath, 1, DbBackup(3, "file.dbs")));
            backupPath.GetFilenames().Should().HaveCount(5).And.Contain(backupFiles.Except(new[] { backupFiles[4] }));

            Assert.Empty(purger.PurgeBackups(backupPath, 1, DbBackup(4, "file.db")));
            backupPath.GetFilenames().Should().HaveCount(5).And.Contain(backupFiles.Except(new[] { backupFiles[4] }));

            Assert.Empty(purger.PurgeBackups(backupPath, 1, DbBackup(1, " ")));
            backupPath.GetFilenames().Should().HaveCount(5).And.Contain(backupFiles.Except(new[] { backupFiles[4] }));

            purger.PurgeBackups(backupPath, 0, DbBackup(3, "file.db")).ToFilenames().Should().HaveCount(1).And.Contain(backupFiles[5]);
            backupPath.GetFilenames().Should().HaveCount(4).And.Contain(backupFiles.Except(new[] { backupFiles[4], backupFiles[5] }));

            DatabaseBackup DbBackup(int id, string name) => new DatabaseBackup { DatabaseBackupId = id, DatabasePath = name };
        }

        private (BackupFilePurger Purger, DirectoryInfo BackupPath) CreateBackupFilePurger(params string[] backupFiles)
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.False(Directory.Exists(tempDirectory));
            _tempDir = Directory.CreateDirectory(tempDirectory);
            foreach (var backupFile in backupFiles)
                using (File.Create(Path.Combine(_tempDir.FullName, backupFile))) { }
            return (new BackupFilePurger(_xunitLogger.CreateLogger<BackupFilePurger>()), _tempDir);
        }

        public void Dispose() => _tempDir?.Delete(true);
    }
}