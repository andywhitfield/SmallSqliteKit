using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data
{
    public class ConfigRepository : IConfigRepository
    {
        private readonly SqliteDataContext _context;

        public ConfigRepository(SqliteDataContext context)
        {
            _context = context;
        }

        public Task<List<Config>> GetAllAsync() => _context.Configs.ToListAsync();

        public Task<Config> FindConfigByNameAsync(string configName) => _context.Configs.Where(c => c.ConfigName == configName).FirstOrDefaultAsync();

        public async Task AddOrUpdateAsync(string configName, string configValue)
        {
            var currentConfig = await FindConfigByNameAsync(configName);
            if (currentConfig != null)
                currentConfig.ConfigValue = configValue;
            else
                await _context.Configs.AddAsync(new Config
                {
                    ConfigName = configName,
                    ConfigValue = configValue
                });
            await _context.SaveChangesAsync();
        }

        public Task DeleteConfigAsync(Config config)
        {
            _context.Configs.Remove(config);
            return _context.SaveChangesAsync();
        }
    }
}