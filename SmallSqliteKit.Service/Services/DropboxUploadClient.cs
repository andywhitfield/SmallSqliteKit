using System;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Data;

namespace SmallSqliteKit.Service.Services
{
    public class DropboxUploadClient : IDropboxUploadClient, IDisposable
    {
        private readonly ILogger<DropboxUploadClient> _logger;
        private readonly IConfigRepository _configRepository;
        private DropboxClient _dropboxClient;

        public DropboxUploadClient(ILogger<DropboxUploadClient> logger, IConfigRepository configRepository)
        {
            _logger = logger;
            _configRepository = configRepository;
        }

        public async Task UploadFileAsync(FileInfo file, string uploadWithFilename)
        {
            if (_dropboxClient == null)
                _dropboxClient = new DropboxClient(await _configRepository.GetDropboxTokenAsync());

            var dropboxFilename = string.IsNullOrWhiteSpace(uploadWithFilename) ? file.Name : uploadWithFilename;
            if (!dropboxFilename.StartsWith('/'))
                dropboxFilename = '/' + dropboxFilename;
            
            using var stream = file.OpenRead();
            var uploadedFile = await _dropboxClient.Files.UploadAsync(dropboxFilename, WriteMode.Overwrite.Instance, body: stream);
            _logger.LogTrace($"Saved {uploadedFile.PathDisplay}/{uploadedFile.Name} rev {uploadedFile.Rev}");
        }

        public void Dispose() => _dropboxClient?.Dispose();
    }
}