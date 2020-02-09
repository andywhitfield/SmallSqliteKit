using System;
using System.Globalization;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Data
{
    public static class DateExtensions
    {
        public const string DefaultDateTimeFormat = "O";

        public static TimeSpan? ToTimeSpan(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            return TimeSpan.TryParse(value, out var ts) ? (TimeSpan?)ts : null;
        }

        public static DateTime NextDateTime(this BackupFrequency backupFrequency, DateTime? fromDate)
        {
            if (fromDate == null)
                return DateTime.MinValue;

            return backupFrequency switch {
                BackupFrequency.Daily => fromDate.Value.AddDays(1),
                BackupFrequency.Weekly => fromDate.Value.AddDays(7),
                BackupFrequency.Monthly => fromDate.Value.AddMonths(1),
                _ => fromDate.Value
            };
        }

        public static DateTime? ToDateTime(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            
            if (DateTime.TryParseExact(value, DefaultDateTimeFormat, null, DateTimeStyles.AssumeUniversal, out var date))
                return date;
            
            return null;
        }
    }
}