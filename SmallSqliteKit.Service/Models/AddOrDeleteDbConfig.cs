namespace SmallSqliteKit.Service.Models
{
    public class AddOrDeleteDbConfig
    {
        public int? Delete { get; set; }
        public int? SaveUpdate { get; set; }
        public int? Update { get; set; }
        public string Add { get; set; }
        public string Cancel { get; set; }
        public DatabaseBackup NewDatabaseModel { get; set; }
    }
}