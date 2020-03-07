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
        
        public DbSet<DatabaseBackup> DatabaseBackups { get; set; }
        
        public DbSet<BackupAudit> BackupAudits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbDataSource = configuration.GetConnectionString("SmallSqliteKit");
            logger.LogDebug($"Using DB connection: {dbDataSource}");
            optionsBuilder.UseSqlite(dbDataSource);
        }
    }
}