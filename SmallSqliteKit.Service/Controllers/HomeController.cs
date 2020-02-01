using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.ViewModels;

namespace SmallSqliteKit.Service.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfigRepository _configRepository;

        public HomeController(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        public async Task<ActionResult> Index()
        {
            var dropboxToken = await _configRepository.GetDropboxTokenAsync();
            return View(new HomeViewModel { IsLinkedToDropbox = !string.IsNullOrEmpty(dropboxToken) });
        }
    }
}