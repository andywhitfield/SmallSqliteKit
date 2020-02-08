namespace SmallSqliteKit.Service.Models
{
    public class AddOrDeleteDbConfig
    {
        public int? Delete { get; set; }
        public string Add { get; set; }
        public DatabaseBackup NewDatabaseModel { get; set; }
    }
}