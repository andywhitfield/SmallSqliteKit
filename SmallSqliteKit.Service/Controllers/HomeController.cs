using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;
using SmallSqliteKit.Service.Services;
using SmallSqliteKit.Service.ViewModels;

namespace SmallSqliteKit.Service.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfigRepository _configRepository;
        private readonly IDatabaseBackupRepository _databaseBackupRepository;
        private readonly IBackupFilePurger _backupFilePurger;
        private readonly IBackupAuditRepository _backupAuditRepository;

        public HomeController(ILogger<HomeController> logger, IConfigRepository configRepository, IDatabaseBackupRepository databaseBackupRepository,
            IBackupFilePurger backupFilePurger, IBackupAuditRepository backupAuditRepository)
        {
            _logger = logger;
            _configRepository = configRepository;
            _databaseBackupRepository = databaseBackupRepository;
            _backupFilePurger = backupFilePurger;
            _backupAuditRepository = backupAuditRepository;
        }

        public async Task<ActionResult> Index([FromQuery]int? update = null)
        {
            var (dropboxAccessToken, dropboxRefreshToken) = await _configRepository.GetDropboxTokensAsync();
            return View(new HomeViewModel
            {
                IsLinkedToDropbox = !string.IsNullOrEmpty(dropboxAccessToken) && !string.IsNullOrEmpty(dropboxRefreshToken),
                BackupPath = (await _configRepository.GetBackupPathAsync()),
                DatabaseBackups = (await _databaseBackupRepository.GetAllAsync()),
                AuditEvents = (await _backupAuditRepository.GetAuditEventsAsync(20)),
                EditingBackupId = update
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Configure([FromForm]string backupPath)
        {
            _logger.LogInformation($"Updating backup path to: [{backupPath}]");
            await _configRepository.SetBackupPathAsync(backupPath);
            return Redirect("~/");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DbConfigs([FromForm]AddOrDeleteDbConfig addOrDeleteDbConfig)
        {
            if (!string.IsNullOrEmpty(addOrDeleteDbConfig.Add) && addOrDeleteDbConfig.NewDatabaseModel != null)
            {
                _logger.LogInformation($"Adding db to backup: {addOrDeleteDbConfig.NewDatabaseModel.DatabasePath}");
                await _databaseBackupRepository.AddAsync(
                    addOrDeleteDbConfig.NewDatabaseModel.DatabasePath,
                    addOrDeleteDbConfig.NewDatabaseModel.BackupFrequency,
                    addOrDeleteDbConfig.NewDatabaseModel.UploadToDropbox,
                    addOrDeleteDbConfig.NewDatabaseModel.UploadToDropboxFrequency
                );
            }

            if (addOrDeleteDbConfig.Delete.HasValue)
            {
                _logger.LogInformation($"Deleting db backup id: {addOrDeleteDbConfig.Delete}");
                var dbToDelete = await _databaseBackupRepository.GetAsync(addOrDeleteDbConfig.Delete.Value);
                if (dbToDelete != null)
                {
                    await _databaseBackupRepository.DeleteAsync(dbToDelete);
                    _backupFilePurger.PurgeBackups(
                        new DirectoryInfo(await _configRepository.GetBackupPathAsync()), 0, dbToDelete);
                }
            }

            if (addOrDeleteDbConfig.Update.HasValue)
            {
                _logger.LogInformation($"Updating db backup id: {addOrDeleteDbConfig.Update}");
                return Redirect($"~/?update={addOrDeleteDbConfig.Update}");
            }

            if (addOrDeleteDbConfig.SaveUpdate.HasValue)
            {
                _logger.LogInformation($"Saving changes to db backup id: {addOrDeleteDbConfig.SaveUpdate}");
                var dbToUpdate = await _databaseBackupRepository.GetAsync(addOrDeleteDbConfig.SaveUpdate.Value);
                if (dbToUpdate != null)
                {
                    dbToUpdate.DatabasePath = addOrDeleteDbConfig.NewDatabaseModel.DatabasePath;
                    dbToUpdate.BackupFrequency = addOrDeleteDbConfig.NewDatabaseModel.BackupFrequency;
                    dbToUpdate.UploadToDropbox = addOrDeleteDbConfig.NewDatabaseModel.UploadToDropbox;
                    dbToUpdate.UploadToDropboxFrequency = addOrDeleteDbConfig.NewDatabaseModel.UploadToDropboxFrequency;
                    dbToUpdate.Optimize = addOrDeleteDbConfig.NewDatabaseModel.Optimize;
                    dbToUpdate.OptimizeFrequency = addOrDeleteDbConfig.NewDatabaseModel.OptimizeFrequency;
                    dbToUpdate.Vacuum = addOrDeleteDbConfig.NewDatabaseModel.Vacuum;
                    dbToUpdate.VacuumFrequency = addOrDeleteDbConfig.NewDatabaseModel.VacuumFrequency;
                    await _databaseBackupRepository.UpdateAsync(dbToUpdate);
                }
            }

            return Redirect("~/");
        }
    }
}