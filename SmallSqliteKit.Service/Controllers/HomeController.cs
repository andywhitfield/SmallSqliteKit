using Microsoft.AspNetCore.Mvc;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;
using SmallSqliteKit.Service.Services;
using SmallSqliteKit.Service.ViewModels;

namespace SmallSqliteKit.Service.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    IConfigRepository configRepository,
    IDatabaseBackupRepository databaseBackupRepository,
    IBackupFilePurger backupFilePurger,
    IBackupAuditRepository backupAuditRepository)
    : Controller
{
    public async Task<ActionResult> Index([FromQuery]int? update = null)
    {
        var (dropboxAccessToken, dropboxRefreshToken) = await configRepository.GetDropboxTokensAsync();
        return View(new HomeViewModel
        {
            IsLinkedToDropbox = !string.IsNullOrEmpty(dropboxAccessToken) && !string.IsNullOrEmpty(dropboxRefreshToken),
            BackupPath = await configRepository.GetBackupPathAsync(),
            DatabaseBackups = await databaseBackupRepository.GetAllAsync(),
            AuditEvents = await backupAuditRepository.GetAuditEventsAsync(20),
            EditingBackupId = update
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Configure([FromForm]string backupPath)
    {
        logger.LogInformation("Updating backup path to: [{BackupPath}]", backupPath);
        await configRepository.SetBackupPathAsync(backupPath);
        return Redirect("~/");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> DbConfigs([FromForm]AddOrDeleteDbConfig addOrDeleteDbConfig)
    {
        if (!string.IsNullOrEmpty(addOrDeleteDbConfig.Add) && addOrDeleteDbConfig.NewDatabaseModel != null)
        {
            logger.LogInformation("Adding db to backup: {DatabasePath}", addOrDeleteDbConfig.NewDatabaseModel.DatabasePath);
            await databaseBackupRepository.AddAsync(
                addOrDeleteDbConfig.NewDatabaseModel.DatabasePath,
                addOrDeleteDbConfig.NewDatabaseModel.BackupFrequency,
                addOrDeleteDbConfig.NewDatabaseModel.UploadToDropbox,
                addOrDeleteDbConfig.NewDatabaseModel.UploadToDropboxFrequency
            );
        }

        if (addOrDeleteDbConfig.Delete.HasValue)
        {
            logger.LogInformation("Deleting db backup id: {Delete}", addOrDeleteDbConfig.Delete);
            var dbToDelete = await databaseBackupRepository.GetAsync(addOrDeleteDbConfig.Delete.Value);
            if (dbToDelete != null)
            {
                await databaseBackupRepository.DeleteAsync(dbToDelete);
                backupFilePurger.PurgeBackups(
                    new DirectoryInfo(await configRepository.GetBackupPathAsync()), 0, dbToDelete);
            }
        }

        if (addOrDeleteDbConfig.Update.HasValue)
        {
            logger.LogInformation("Updating db backup id: {Update}", addOrDeleteDbConfig.Update);
            return Redirect($"~/?update={addOrDeleteDbConfig.Update}");
        }

        if (addOrDeleteDbConfig.SaveUpdate.HasValue)
        {
            logger.LogInformation("Saving changes to db backup id: {SaveUpdate}", addOrDeleteDbConfig.SaveUpdate);
            var dbToUpdate = await databaseBackupRepository.GetAsync(addOrDeleteDbConfig.SaveUpdate.Value);
            if (dbToUpdate != null && addOrDeleteDbConfig.NewDatabaseModel != null)
            {
                dbToUpdate.DatabasePath = addOrDeleteDbConfig.NewDatabaseModel.DatabasePath;
                dbToUpdate.BackupFrequency = addOrDeleteDbConfig.NewDatabaseModel.BackupFrequency;
                dbToUpdate.UploadToDropbox = addOrDeleteDbConfig.NewDatabaseModel.UploadToDropbox;
                dbToUpdate.UploadToDropboxFrequency = addOrDeleteDbConfig.NewDatabaseModel.UploadToDropboxFrequency;
                dbToUpdate.Optimize = addOrDeleteDbConfig.NewDatabaseModel.Optimize;
                dbToUpdate.OptimizeFrequency = addOrDeleteDbConfig.NewDatabaseModel.OptimizeFrequency;
                dbToUpdate.Vacuum = addOrDeleteDbConfig.NewDatabaseModel.Vacuum;
                dbToUpdate.VacuumFrequency = addOrDeleteDbConfig.NewDatabaseModel.VacuumFrequency;
                await databaseBackupRepository.UpdateAsync(dbToUpdate);
            }
        }

        return Redirect("~/");
    }
}