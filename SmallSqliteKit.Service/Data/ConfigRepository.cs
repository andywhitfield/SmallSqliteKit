using Microsoft.EntityFrameworkCore;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data;

public class ConfigRepository(SqliteDataContext context) : IConfigRepository
{
    public Task<List<Config>> GetAllAsync() => context.Configs.ToListAsync();

    public Task<Config?> FindConfigByNameAsync(string configName) => context.Configs.Where(c => c.ConfigName == configName).FirstOrDefaultAsync();

    public async Task AddOrUpdateAsync(string configName, string configValue)
    {
        var currentConfig = await FindConfigByNameAsync(configName);
        if (currentConfig != null)
            currentConfig.ConfigValue = configValue;
        else
            await context.Configs.AddAsync(new Config
            {
                ConfigName = configName,
                ConfigValue = configValue
            });
        await context.SaveChangesAsync();
    }

    public Task DeleteConfigAsync(Config config)
    {
        context.Configs.Remove(config);
        return context.SaveChangesAsync();
    }
}