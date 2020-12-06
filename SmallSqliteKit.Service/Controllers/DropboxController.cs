using System;
using System.Threading.Tasks;
using Dropbox.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Services;

namespace SmallSqliteKit.Service.Controllers
{
    public class DropboxController : Controller
    {
        private readonly IConfigRepository _configRepository;
        private readonly ILogger<DropboxController> _logger;
        private readonly DropboxOptions _dropboxOptions;

        public DropboxController(IConfigRepository configRepository,
            ILogger<DropboxController> logger,
            IOptions<DropboxOptions> dropboxOptions)
        {
            _configRepository = configRepository;
            _logger = logger;
            _dropboxOptions = dropboxOptions.Value;
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
            var dropboxRedirect = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, _dropboxOptions.SmallSqliteKitAppKey, RedirectUri, tokenAccessType: TokenAccessType.Offline, scopeList: new[] {"files.content.write"});
            _logger.LogInformation($"Getting user token from Dropbox: {dropboxRedirect} (redirect={RedirectUri})");
            return Redirect(dropboxRedirect.ToString());
        }

        [HttpGet]
        public async Task<ActionResult> Authentication(string code, string state)
        {
            var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, _dropboxOptions.SmallSqliteKitAppKey, _dropboxOptions.SmallSqliteKitAppSecret, RedirectUri.ToString());
            _logger.LogInformation($"Got user token from Dropbox: {response.AccessToken}");

            await _configRepository.SetDropboxTokensAsync(response.AccessToken, response.RefreshToken);
            return Redirect("~/");
        }

        [HttpGet]
        public async Task<ActionResult> Disconnect()
        {
            await _configRepository.SetDropboxTokensAsync(null, null);
            return Redirect("~/");
        }
    }
}