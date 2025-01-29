using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;

namespace SmallSqliteKit.Service.Tests.Data
{
    [TestClass]
    public class DateExtensionsTest
    {
        [TestMethod]
        [DataRow("0", "00:00:00")]
        [DataRow("14", "14.00:00:00")]
        [DataRow("1:2:3", "01:02:03")]
        [DataRow("0:0:0.250", "00:00:00.25")]
        [DataRow("10.12:00", "10.12:00:00")]
        public void Can_parse_valid_timespan(string value, string expectedValue)
        {
            Assert.AreEqual(TimeSpan.Parse(expectedValue), DateExtensions.ToTimeSpan(value));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("    ")]
        [DataRow("0:60:0")]
        [DataRow("0:0:60")]
        [DataRow("10:")]
        [DataRow(":10")]
        [DataRow(".123")]
        [DataRow("10.")]
        [DataRow("10.12")]
        public void Invalid_timespan_returns_null(string value)
        {
            Assert.IsNull(DateExtensions.ToTimeSpan(value));
        }

        [TestMethod]
        [DataRow("2009-06-15T13:45:30.0000000-07:00", "2009-06-15 20:45:30Z")]
        [DataRow("2009-06-15T20:45:30.0000000", "2009-06-15 20:45:30Z")]
        [DataRow("2009-06-15T20:45:30.0000000Z", "2009-06-15 20:45:30Z")]
        public void Can_parse_valid_datetime(string value, string expectedValue)
        {
            Assert.AreEqual(DateTime.ParseExact(expectedValue, "u", null, DateTimeStyles.AdjustToUniversal), DateExtensions.ToDateTime(value));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("    ")]
        [DataRow("2009-06-15 13:45:30.0000000-07:00")]
        [DataRow("2009-06-15T20:45:30.0000000-23:00")]
        [DataRow("2009-06-15T20:45:30.0000000Y")]
        [DataRow("2009-06-15")]
        [DataRow("20:45:30")]
        public void Invalid_datetime_returns_null(string value)
        {
            Assert.IsNull(DateExtensions.ToDateTime(value));
        }

        [TestMethod]
        public void When_datetime_is_null_Then_next_datetime_is_minvalue()
        {
            Assert.AreEqual(DateTime.MinValue, DateExtensions.NextDateTime(BackupFrequency.Daily, null));
            Assert.AreEqual(DateTime.MinValue, DateExtensions.NextDateTime(BackupFrequency.Weekly, null));
            Assert.AreEqual(DateTime.MinValue, DateExtensions.NextDateTime(BackupFrequency.Monthly, null));
        }

        [TestMethod]
        [DataRow(BackupFrequency.Daily, "2020-02-29T20:45:30.0000000Z", "2020-03-01T20:45:30.0000000Z")]
        [DataRow(BackupFrequency.Weekly, "2020-02-29T20:45:30.0000000Z", "2020-03-07T20:45:30.0000000Z")]
        [DataRow(BackupFrequency.Monthly, "2020-02-29T20:45:30.0000000Z", "2020-03-29T20:45:30.0000000Z")]
        [DataRow(BackupFrequency.Monthly, "2020-01-29T20:45:30.0000000Z", "2020-02-29T20:45:30.0000000Z")]
        [DataRow(BackupFrequency.Monthly, "2020-01-30T20:45:30.0000000Z", "2020-02-29T20:45:30.0000000Z")]
        [DataRow(BackupFrequency.Monthly, "2020-01-31T20:45:30.0000000Z", "2020-02-29T20:45:30.0000000Z")]
        [DataRow(BackupFrequency.Daily, "2019-12-31T20:45:30.0000000Z", "2020-01-01T20:45:30.0000000Z")]
        [DataRow(BackupFrequency.Weekly, "2019-12-31T20:45:30.0000000Z", "2020-01-07T20:45:30.0000000Z")]
        [DataRow(BackupFrequency.Monthly, "2019-12-31T20:45:30.0000000Z", "2020-01-31T20:45:30.0000000Z")]
        public void Should_return_next_expected_date(BackupFrequency backupFrequency, string date, string expectedNextDate)
        {
            var dateValue = DateTime.ParseExact(date, "o", null, DateTimeStyles.RoundtripKind);
            var expectedNextDateValue = DateTime.ParseExact(expectedNextDate, "o", null, DateTimeStyles.RoundtripKind);
            Assert.AreEqual(expectedNextDateValue, DateExtensions.NextDateTime(backupFrequency, dateValue));
        }
    }
}