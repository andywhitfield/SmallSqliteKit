using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmallSqliteKit.Service.Data;

namespace SmallSqliteKit.Service.Services
{
    public class DropboxUploadClient : IDropboxUploadClient, IDisposable
    {
        private readonly ILogger<DropboxUploadClient> _logger;
        private readonly IConfigRepository _configRepository;
        private readonly DropboxOptions _dropboxOptions;
        private DropboxClient _dropboxClient;

        public DropboxUploadClient(ILogger<DropboxUploadClient> logger, IConfigRepository configRepository, IOptions<DropboxOptions> dropboxOptions)
        {
            _logger = logger;
            _configRepository = configRepository;
            _dropboxOptions = dropboxOptions.Value;
        }

        public async Task UploadFileAsync(FileInfo file, string uploadWithFilename)
        {
            if (_dropboxClient == null)
            {
                var (accessToken, refreshToken) = await _configRepository.GetDropboxTokensAsync();
                _dropboxClient = new DropboxClient(accessToken, refreshToken, _dropboxOptions.SmallSqliteKitAppKey,
                    _dropboxOptions.SmallSqliteKitAppSecret, new DropboxClientConfig());

                if (!await _dropboxClient.RefreshAccessToken(new[] { "files.content.write" }))
                {
                    _logger.LogWarning($"Could not refresh Dropbox access token");
                    return;
                }
            }

            var dropboxFilename = string.IsNullOrWhiteSpace(uploadWithFilename) ? file.Name : uploadWithFilename;
            dropboxFilename = $"{(dropboxFilename.StartsWith('/') ? string.Empty : "/")}{dropboxFilename}.gz";

            using var fileToUploadStream = file.OpenRead();
            using var outputStream = new MemoryStream();
            using (var stream = new GZipStream(outputStream, CompressionLevel.Optimal, true))
                await fileToUploadStream.CopyToAsync(stream);

            outputStream.Position = 0;
            var uploadedFile = await _dropboxClient.Files.UploadAsync(dropboxFilename, WriteMode.Overwrite.Instance, body: outputStream);
            _logger.LogTrace($"Saved {uploadedFile.PathDisplay}/{uploadedFile.Name} rev {uploadedFile.Rev}");
        }

        public void Dispose() => _dropboxClient?.Dispose();
    }
}