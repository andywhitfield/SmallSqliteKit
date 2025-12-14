using System.IO.Compression;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Options;
using SmallSqliteKit.Service.Data;

namespace SmallSqliteKit.Service.Services;

public class DropboxUploadClient(ILogger<DropboxUploadClient> logger, IConfigRepository configRepository, IOptions<DropboxOptions> dropboxOptions)
    : IDropboxUploadClient, IDisposable
{
    private readonly DropboxOptions _dropboxOptions = dropboxOptions.Value;
    private DropboxClient? _dropboxClient;

    public async Task UploadFileAsync(FileInfo file, string uploadWithFilename)
    {
        if (_dropboxClient == null)
        {
            var (accessToken, refreshToken) = await configRepository.GetDropboxTokensAsync();
            _dropboxClient = new DropboxClient(accessToken, refreshToken, _dropboxOptions.SmallSqliteKitAppKey,
                _dropboxOptions.SmallSqliteKitAppSecret, new DropboxClientConfig());

            if (!await _dropboxClient.RefreshAccessToken(["files.content.write"]))
            {
                logger.LogWarning("Could not refresh Dropbox access token");
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
        logger.LogTrace("Saved {PathDisplay}/{Name} rev {Rev}", uploadedFile.PathDisplay, uploadedFile.Name, uploadedFile.Rev);
    }

    public void Dispose() => _dropboxClient?.Dispose();
}