using System;
using System.Globalization;
using SmallSqliteKit.Service.Data;
using SmallSqliteKit.Service.Models;
using Xunit;

namespace SmallSqliteKit.Service.Tests.Data
{
    public class DateExtensionsTest
    {
        [Theory]
        [InlineData("0", "00:00:00")]
        [InlineData("14", "14.00:00:00")]
        [InlineData("1:2:3", "01:02:03")]
        [InlineData("0:0:0.250", "00:00:00.25")]
        [InlineData("10.12:00", "10.12:00:00")]
        public void Can_parse_valid_timespan(string value, string expectedValue)
        {
            Assert.Equal(TimeSpan.Parse(expectedValue), DateExtensions.ToTimeSpan(value));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData("0:60:0")]
        [InlineData("0:0:60")]
        [InlineData("10:")]
        [InlineData(":10")]
        [InlineData(".123")]
        [InlineData("10.")]
        [InlineData("10.12")]
        public void Invalid_timespan_returns_null(string value)
        {
            Assert.Null(DateExtensions.ToTimeSpan(value));
        }

        [Theory]
        [InlineData("2009-06-15T13:45:30.0000000-07:00", "2009-06-15 20:45:30Z")]
        [InlineData("2009-06-15T20:45:30.0000000", "2009-06-15 20:45:30Z")]
        [InlineData("2009-06-15T20:45:30.0000000Z", "2009-06-15 20:45:30Z")]
        public void Can_parse_valid_datetime(string value, string expectedValue)
        {
            Assert.Equal(DateTime.ParseExact(expectedValue, "u", null, DateTimeStyles.AdjustToUniversal), DateExtensions.ToDateTime(value));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData("2009-06-15 13:45:30.0000000-07:00")]
        [InlineData("2009-06-15T20:45:30.0000000-23:00")]
        [InlineData("2009-06-15T20:45:30.0000000Y")]
        [InlineData("2009-06-15")]
        [InlineData("20:45:30")]
        public void Invalid_datetime_returns_null(string value)
        {
            Assert.Null(DateExtensions.ToDateTime(value));
        }

        [Fact]
        public void When_datetime_is_null_Then_next_datetime_is_minvalue()
        {
            Assert.Equal(DateTime.MinValue, DateExtensions.NextDateTime(BackupFrequency.Daily, null));
            Assert.Equal(DateTime.MinValue, DateExtensions.NextDateTime(BackupFrequency.Weekly, null));
            Assert.Equal(DateTime.MinValue, DateExtensions.NextDateTime(BackupFrequency.Monthly, null));
        }

        [Theory]
        [InlineData(BackupFrequency.Daily, "2020-02-29T20:45:30.0000000Z", "2020-03-01T20:45:30.0000000Z")]
        [InlineData(BackupFrequency.Weekly, "2020-02-29T20:45:30.0000000Z", "2020-03-07T20:45:30.0000000Z")]
        [InlineData(BackupFrequency.Monthly, "2020-02-29T20:45:30.0000000Z", "2020-03-29T20:45:30.0000000Z")]
        [InlineData(BackupFrequency.Monthly, "2020-01-29T20:45:30.0000000Z", "2020-02-29T20:45:30.0000000Z")]
        [InlineData(BackupFrequency.Monthly, "2020-01-30T20:45:30.0000000Z", "2020-02-29T20:45:30.0000000Z")]
        [InlineData(BackupFrequency.Monthly, "2020-01-31T20:45:30.0000000Z", "2020-02-29T20:45:30.0000000Z")]
        [InlineData(BackupFrequency.Daily, "2019-12-31T20:45:30.0000000Z", "2020-01-01T20:45:30.0000000Z")]
        [InlineData(BackupFrequency.Weekly, "2019-12-31T20:45:30.0000000Z", "2020-01-07T20:45:30.0000000Z")]
        [InlineData(BackupFrequency.Monthly, "2019-12-31T20:45:30.0000000Z", "2020-01-31T20:45:30.0000000Z")]
        public void Should_return_next_expected_date(BackupFrequency backupFrequency, string date, string expectedNextDate)
        {
            var dateValue = DateTime.ParseExact(date, "o", null, DateTimeStyles.RoundtripKind);
            var expectedNextDateValue = DateTime.ParseExact(expectedNextDate, "o", null, DateTimeStyles.RoundtripKind);
            Assert.Equal(expectedNextDateValue, DateExtensions.NextDateTime(backupFrequency, dateValue));
        }
    }
}