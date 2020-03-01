using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmallSqliteKit.Service.Models
{
    public class BackupAudit
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BackupAuditId { get; set; }
        [Required]
        public DatabaseBackup DatabaseBackup { get; set; }
        [Required]
        public string AuditLog { get; set; }
        [Required]
        public DateTime TimestampCreated { get; set; }
    }
}