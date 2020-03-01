using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SmallSqliteKit.Service.Controllers;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;
using SmallSqliteKit.Service.Services;
using Xunit;

namespace SmallSqliteKit.Service.Tests.Controllers
{
    public class HomeControllerTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public async Task When_AddingNewConfig_Should_UpdateRepository()
        {
            var databaseBackupRepository = new Mock<IDatabaseBackupRepository>();
            var backupFilePurger = new Mock<IBackupFilePurger>(MockBehavior.Strict);
            var controller = new HomeController(Mock.Of<ILogger<HomeController>>(), Mock.Of<IConfigRepository>(),
                databaseBackupRepository.Object, backupFilePurger.Object, Mock.Of<IBackupAuditRepository>());

            var request = new AddOrDeleteDbConfig
            {
                Add = "add",
                NewDatabaseModel = _fixture.Create<DatabaseBackup>()
            };
            var response = await controller.DbConfigs(request);

            Assert.IsType<RedirectResult>(response);
            var redirect = (RedirectResult)response;
            Assert.Equal("~/", redirect.Url);
            Assert.False(redirect.Permanent);

            databaseBackupRepository.Verify(d => d.AddAsync(request.NewDatabaseModel.DatabasePath,
                request.NewDatabaseModel.BackupFrequency, request.NewDatabaseModel.UploadToDropbox,
                request.NewDatabaseModel.UploadToDropboxFrequency), Times.Once);
            databaseBackupRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task When_DeletingDbConfig_Should_RemoveFromRepositoryAndPurge()
        {
            var databaseBackupRepository = new Mock<IDatabaseBackupRepository>();
            var backupFilePurger = new Mock<IBackupFilePurger>();
            var controller = new HomeController(Mock.Of<ILogger<HomeController>>(), Mock.Of<IConfigRepository>(),
                databaseBackupRepository.Object, backupFilePurger.Object, Mock.Of<IBackupAuditRepository>());

            var dbBackup = _fixture
                .Build<DatabaseBackup>()
                .With(x => x.DatabasePath, Path.Combine(_fixture.CreateMany<string>().ToArray()))
                .Create();
            var request = new AddOrDeleteDbConfig { Delete = dbBackup.DatabaseBackupId };
            databaseBackupRepository.Setup(r => r.GetAsync(request.Delete.Value)).ReturnsAsync(dbBackup);

            var response = await controller.DbConfigs(request);

            Assert.IsType<RedirectResult>(response);
            var redirect = (RedirectResult)response;
            Assert.Equal("~/", redirect.Url);
            Assert.False(redirect.Permanent);

            databaseBackupRepository.Verify(d => d.DeleteAsync(dbBackup), Times.Once);
            backupFilePurger.Verify(p => p.PurgeBackups(It.IsAny<DirectoryInfo>(), 0, Path.GetFileName(dbBackup.DatabasePath)), Times.Once);
            backupFilePurger.VerifyNoOtherCalls();
        }
    }
}