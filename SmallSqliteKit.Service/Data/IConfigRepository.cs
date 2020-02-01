using System.Collections.Generic;
using System.Threading.Tasks;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data
{
    public interface IConfigRepository
    {
        Task<List<Config>> GetAllAsync();
        Task<Config> FindConfigByNameAsync(string configName);
        Task AddOrUpdateAsync(string configName, string configValue);
        Task DeleteConfigAsync(Config config);
    }
}