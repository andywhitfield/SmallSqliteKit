using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.ViewModels;

namespace SmallSqliteKit.Service.Controllers
{
    public class HomeController : Controller
    {
        private const string ConfigNameDropboxToken = "DropboxToken";

        private readonly IConfigRepository _configRepository;

        public HomeController(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        public async Task<ActionResult> Index()
        {
            var model = new HomeViewModel();

            var dropboxTokenConfig = await _configRepository.FindConfigByNameAsync(ConfigNameDropboxToken);
            if (!string.IsNullOrEmpty(dropboxTokenConfig?.ConfigValue))
                model.IsLinkedToDropbox = true;

            return View(model);
        }
    }
}