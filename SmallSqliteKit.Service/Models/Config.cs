using System.ComponentModel.DataAnnotations;

namespace SmallSqliteKit.Service.Models
{
    public class Config
    {
        [Key, Required]
        public string ConfigName { get; set; }
        public string ConfigValue { get; set; }
    }
}