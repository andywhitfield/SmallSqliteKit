using System.Threading.Tasks;

namespace SmallSqliteKit.Service.Data
{
    public static class ConfigRepositoryExtensions
    {
        private const string ConfigNameDropboxToken = "DropboxToken";
        private const string ConfigNameBackupPath = "BackupPath";

        public static async Task<string> GetDropboxTokenAsync(this IConfigRepository configRepository)
        {
            var dropboxTokenConfig = await configRepository.FindConfigByNameAsync(ConfigNameDropboxToken);
            return dropboxTokenConfig?.ConfigValue;
        }

        public static async Task SetDropboxTokenAsync(this IConfigRepository configRepository, string dropboxToken)
        {
            if (string.IsNullOrEmpty(dropboxToken))
            {
                var dropboxTokenConfig = await configRepository.FindConfigByNameAsync(ConfigNameDropboxToken);
                if (dropboxTokenConfig != null)
                    await configRepository.DeleteConfigAsync(dropboxTokenConfig);
            }
            else
            {
                await configRepository.AddOrUpdateAsync(ConfigNameDropboxToken, dropboxToken);
            }
        }

        public static async Task<string> GetBackupPathAsync(this IConfigRepository configRepository)
        {
            return (await configRepository.FindConfigByNameAsync(ConfigNameBackupPath))?.ConfigValue ?? string.Empty;
        }

        public static async Task SetBackupPathAsync(this IConfigRepository configRepository, string backupPath)
        {
            if (string.IsNullOrEmpty(backupPath))
            {
                var backupPathConfig = await configRepository.FindConfigByNameAsync(ConfigNameBackupPath);
                if (backupPathConfig != null)
                    await configRepository.DeleteConfigAsync(backupPathConfig);
            }
            else
            {
                await configRepository.AddOrUpdateAsync(ConfigNameBackupPath, backupPath);
            }
        }
    }
}