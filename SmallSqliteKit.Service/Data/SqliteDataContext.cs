using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data
{
    public class SqliteDataContext : DbContext
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<SqliteDataContext> logger;

        public SqliteDataContext(IConfiguration configuration, ILogger<SqliteDataContext> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }
        
        public DbSet<Config> Configs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbDataSource = configuration["DbContext:DbPath"];
            logger.LogDebug($"Using DB location: {dbDataSource}");
            optionsBuilder.UseSqlite($"Data Source={dbDataSource}");
        }
    }
}