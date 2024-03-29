﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmallSqliteKit.Service.Models
{
    public class DatabaseBackup
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DatabaseBackupId { get; set; }
        [Required]
        public string DatabasePath { get; set; }
        public BackupFrequency BackupFrequency { get; set; }
        public DateTime? LastBackupTime { get; set; }
        
        public bool UploadToDropbox { get; set; }
        public BackupFrequency? UploadToDropboxFrequency { get; set; }
        public DateTime? LastUploadToDropboxTime { get; set; }
        
        public bool Optimize { get; set; }
        public BackupFrequency? OptimizeFrequency { get; set; }
        public DateTime? LastOptimizeTime { get; set; }
        
        public bool Vacuum { get; set; }
        public BackupFrequency? VacuumFrequency { get; set; }
        public DateTime? LastVacuumTime { get; set; }
    }
}