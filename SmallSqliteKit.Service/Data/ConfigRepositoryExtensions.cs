using System.Threading.Tasks;

namespace SmallSqliteKit.Service.Data
{
    public static class ConfigRepositoryExtensions
    {
        private const string ConfigNameDropboxAccessToken = "DropboxAccessToken";
        private const string ConfigNameDropboxRefreshToken = "DropboxRefreshToken";
        private const string ConfigNameBackupPath = "BackupPath";
        private const string ConfigNameBackupFileCount = "BackupFileCount";

        public static async Task<(string AccessToken, string RefreshToken)> GetDropboxTokensAsync(this IConfigRepository configRepository)
        {
            var dropboxAccessTokenConfig = await configRepository.FindConfigByNameAsync(ConfigNameDropboxAccessToken);
            var dropboxRefreshTokenConfig = await configRepository.FindConfigByNameAsync(ConfigNameDropboxRefreshToken);
            return (dropboxAccessTokenConfig?.ConfigValue, dropboxRefreshTokenConfig?.ConfigValue);
        }

        public static async Task SetDropboxTokensAsync(this IConfigRepository configRepository, string dropboxAccessToken, string dropboxRefreshToken)
        {
            await configRepository.SetConfigValueAsync(ConfigNameDropboxAccessToken, dropboxAccessToken);
            await configRepository.SetConfigValueAsync(ConfigNameDropboxRefreshToken, dropboxRefreshToken);
        }

        public static async Task<string> GetBackupPathAsync(this IConfigRepository configRepository)
            => (await configRepository.FindConfigByNameAsync(ConfigNameBackupPath))?.ConfigValue ?? ".";

        public static Task SetBackupPathAsync(this IConfigRepository configRepository, string backupPath)
            => configRepository.SetConfigValueAsync(ConfigNameBackupPath, backupPath);

        public static async Task<int> GetBackupFileCountAsync(this IConfigRepository configRepository)
            => int.TryParse((await configRepository.FindConfigByNameAsync(ConfigNameBackupFileCount))?.ConfigValue ?? string.Empty, out var fileCount) ? fileCount : 5;

        private static async Task SetConfigValueAsync(this IConfigRepository configRepository, string configName, string configValue)
        {
            if (string.IsNullOrEmpty(configValue))
            {
                var existingConfig = await configRepository.FindConfigByNameAsync(configName);
                if (existingConfig != null)
                    await configRepository.DeleteConfigAsync(existingConfig);
            }
            else
            {
                await configRepository.AddOrUpdateAsync(configName, configValue);
            }            
        }
    }
}