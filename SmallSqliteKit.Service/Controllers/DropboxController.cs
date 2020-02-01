using System;
using System.Threading.Tasks;
using Dropbox.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Data;

namespace SmallSqliteKit.Service.Controllers
{
    public class DropboxController : Controller
    {
        private readonly IConfigRepository _configRepository;
        private readonly ILogger<DropboxController> _logger;
        private readonly string _dropboxAppKey;
        private readonly string _dropboxAppSecret;

        public DropboxController(IConfigRepository configRepository,
            ILogger<DropboxController> logger,
            IConfiguration configuration)
        {
            _configRepository = configRepository;
            _logger = logger;
            _dropboxAppKey = configuration["Dropbox:SmallSqliteKitAppKey"];
            _dropboxAppSecret = configuration["Dropbox:SmallSqliteKitAppSecret"];
        }

        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder();
                uriBuilder.Scheme = Request.Scheme;
                uriBuilder.Host = Request.Host.Host;
                if (Request.Host.Port.HasValue && Request.Host.Port != 443 && Request.Host.Port != 80)
                    uriBuilder.Port = Request.Host.Port.Value;
                uriBuilder.Path = "dropbox/authentication";
                return uriBuilder.Uri;
            }
        }

        [HttpGet]
        public IActionResult Connect()
        {
            var dropboxRedirect = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, _dropboxAppKey, RedirectUri);
            _logger.LogInformation($"Getting user token from Dropbox: {dropboxRedirect} (redirect={RedirectUri})");
            return Redirect(dropboxRedirect.ToString());
        }

        [HttpGet]
        public async Task<ActionResult> Authentication(string code, string state)
        {
            var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, _dropboxAppKey, _dropboxAppSecret, RedirectUri.ToString());
            _logger.LogInformation($"Got user token from Dropbox: {response.AccessToken}");

            await _configRepository.SetDropboxTokenAsync(response.AccessToken);
            return Redirect("~/");
        }

        [HttpGet]
        public async Task<ActionResult> Disconnect()
        {
            await _configRepository.SetDropboxTokenAsync(null);
            return Redirect("~/");
        }
    }
}