using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.ViewModels;

namespace SmallSqliteKit.Service.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfigRepository _configRepository;

        public HomeController(ILogger<HomeController> logger, IConfigRepository configRepository)
        {
            _logger = logger;
            _configRepository = configRepository;
        }

        public async Task<ActionResult> Index()
        {
            var dropboxToken = await _configRepository.GetDropboxTokenAsync();
            return View(new HomeViewModel
            {
                IsLinkedToDropbox = !string.IsNullOrEmpty(dropboxToken),
                BackupPath = (await _configRepository.GetBackupPathAsync())
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Configure([FromForm]string backupPath)
        {
            _logger.LogInformation($"Updating backup path to: [{backupPath}]");
            await _configRepository.SetBackupPathAsync(backupPath);
            return Redirect("~/");
        }
    }
}