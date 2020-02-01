using System.Threading.Tasks;

namespace SmallSqliteKit.Service.Data
{
    public static class ConfigRepositoryExtensions
    {
        private const string ConfigNameDropboxToken = "DropboxToken";

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
    }
}