using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using static WinFormsLib.Constants;

namespace WinFormsLib
{
    /// <summary>Provides date formatting, JSON conversion, and calendar helpers.</summary>
    public static class DateTimeExtensions
    {
        /// <summary>The format for a date and time including milliseconds.</summary>
        public const string FORMAT_DATE_TIME = "yyyy-MM-dd HH.mm.ss fff";
        /// <summary>The format for a date and time through seconds.</summary>
        public const string FORMAT_DATE_SECOND = "yyyy-MM-dd HH.mm.ss";
        /// <summary>The format for a date and time through minutes.</summary>
        public const string FORMAT_DATE_HOUR = "yyyy-MM-dd HH.mm";
        /// <summary>The format for a full date without a time.</summary>
        public const string FORMAT_DATE_DAY = "yyyy-MM-dd";
        /// <summary>The format for a year and month.</summary>
        public const string FORMAT_DATE_MONTH = "yyyy-MM";
        /// <summary>The format for a four-digit year.</summary>
        public const string FORMAT_DATE_YEAR = "yyyy";

        /// <summary>The format for a 24-hour time through minutes.</summary>
        public const string FORMAT_HOUR = "HH:mm";
        /// <summary>The format for an abbreviated weekday name.</summary>
        public const string FORMAT_DAY_OF_WEEK_SHORT = "ddd";
        /// <summary>The format for a full weekday name.</summary>
        public const string FORMAT_DAY_OF_WEEK_LONG = "dddd";
        /// <summary>The format for the day of the month followed by a space.</summary>
        public const string FORMAT_DAY_OF_MONTH_NUMBER = "d ";

        /// <summary>The baseline number of weeks in a week-numbering year.</summary>
        public const int NUMBER_OF_WEEKS_IN_A_YEAR = 52;
        /// <summary>The number of days in a week.</summary>
        public const int NUMBER_OF_DAYS_IN_A_WEEK = 7;

        /// <summary>Converts a date and time including milliseconds to and from a JSON string.</summary>
        public static readonly JsonConverter DateTimeJsonConverter = new DateTime_JsonConverter();
        /// <summary>Converts a date and time through seconds to and from a JSON string.</summary>
        public static readonly JsonConverter DateSecondJsonConverter = new DateSecond_JsonConverter();
        /// <summary>Converts a date and time through minutes to and from a JSON string.</summary>
        public static readonly JsonConverter DateHourJsonConverter = new DateHour_JsonConverter();
        /// <summary>Converts a full date to and from a JSON string.</summary>
        public static readonly JsonConverter DateDayJsonConverter = new DateDay_JsonConverter();
        /// <summary>Converts a year and month to and from a JSON string.</summary>
        public static readonly JsonConverter DateMonthJsonConverter = new DateMonth_JsonConverter();
        /// <summary>Converts a year to and from a JSON string.</summary>
        public static readonly JsonConverter DateYearJsonConverter = new DateYear_JsonConverter();

        /// <summary>Selects a date or time format at the specified precision.</summary>
        public enum DateTimeFormat
        {
            /// <summary>Uses the format for a date and time including milliseconds.</summary>
            [Utils.Value(FORMAT_DATE_TIME)]
            Time = 0,
            /// <summary>Uses the format for a date and time through seconds.</summary>
            [Utils.Value(FORMAT_DATE_SECOND)]
            Second = 1,
            /// <summary>Uses the format for a date and time through minutes.</summary>
            [Utils.Value(FORMAT_DATE_HOUR)]
            Hour = 2,
            /// <summary>Uses the format for a full date.</summary>
            [Utils.Value(FORMAT_DATE_DAY)]
            Day = 3,
            /// <summary>Uses the format for a year and month.</summary>
            [Utils.Value(FORMAT_DATE_MONTH)]
            Month = 4,
            /// <summary>Uses the format for a year.</summary>
            [Utils.Value(FORMAT_DATE_YEAR)]
            Year = 5
        };

        private class DateTime_JsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s) && s.AsDateTime(DateTimeFormat.Time) is DateTime dt ? dt : default;
            }
            public override void Write(Utf8JsonWriter writer, DateTime dateTime, JsonSerializerOptions options) => writer.WriteStringValue(dateTime.ToDateTimeString());
        }
        private class DateSecond_JsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s) && s.AsDateTime(DateTimeFormat.Second) is DateTime dt ? dt : default;
            }
            public override void Write(Utf8JsonWriter writer, DateTime dateTime, JsonSerializerOptions options) => writer.WriteStringValue(dateTime.ToDateSecondString());
        }
        private class DateHour_JsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s) && s.AsDateTime(DateTimeFormat.Hour) is DateTime dt ? dt : default;
            }
            public override void Write(Utf8JsonWriter writer, DateTime dateTime, JsonSerializerOptions options) => writer.WriteStringValue(dateTime.ToDateHourString());
        }
        private class DateDay_JsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s) && s.AsDateTime(DateTimeFormat.Day) is DateTime dt ? dt : default;
            }
            public override void Write(Utf8JsonWriter writer, DateTime dateTime, JsonSerializerOptions options) => writer.WriteStringValue(dateTime.ToDateDayString());
        }
        private class DateMonth_JsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s) && s.AsDateTime(DateTimeFormat.Month) is DateTime dt ? dt : default;
            }
            public override void Write(Utf8JsonWriter writer, DateTime dateTime, JsonSerializerOptions options) => writer.WriteStringValue(dateTime.ToDateMonthString());
        }
        private class DateYear_JsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s) && s.AsDateTime(DateTimeFormat.Year) is DateTime dt ? dt : default;
            }
            public override void Write(Utf8JsonWriter writer, DateTime dateTime, JsonSerializerOptions options) => writer.WriteStringValue(dateTime.ToDateYearString());
        }

        /// <summary>Determines whether the year of the supplied date is a leap year.</summary>
        /// <param name="super">The source date and time.</param>
        public static bool IsLeapYear(this DateTime super) => DateTime.IsLeapYear(super.Year);

        /// <summary>Returns the number of days in the year of the supplied date.</summary>
        /// <param name="super">The source date and time.</param>
        public static int GetDaysInYear(this DateTime super) => CULTURE_INFO_DEFAULT.Calendar.GetDaysInYear(super.Year);

        /// <summary>Returns the number of days in the month of the supplied date.</summary>
        /// <param name="super">The source date and time.</param>
        public static int GetDaysInMonth(this DateTime super) => DateTime.DaysInMonth(super.Year, super.Month);

        /// <summary>Returns a zero-based weekday index.</summary>
        /// <param name="super">The source date and time.</param>
        /// <param name="firstDaySunday">Whether Sunday is index zero. Otherwise, Monday is index zero.</param>
        public static int GetDayOfWeek(this DateTime super, bool firstDaySunday = false)
        {
            int i = (int)super.DayOfWeek;
            if (!firstDaySunday)
            {
                i = i != 0 ? i - 1 : 6;
            }
            return i;
        }

        /// <summary>Returns the number of weeks in the ISO week-numbering year that contains the date.</summary>
        /// <param name="super">The source date and time.</param>
        public static int GetWeeksInYear(this DateTime super) => ISOWeek.GetWeeksInYear(ISOWeek.GetYear(super));

        /// <summary>Returns the ISO week number of the date.</summary>
        /// <param name="super">The source date and time.</param>
        public static int GetWeek(this DateTime super) => ISOWeek.GetWeekOfYear(super);

        /// <summary>Returns the Monday of the specified week in the date's ISO week-numbering year.</summary>
        /// <param name="super">The source date and time.</param>
        /// <param name="week">The ISO week number, or -1 to use the date's own week number.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified week does not exist in the date's ISO week-numbering year.</exception>
        public static DateTime GetFirstDateOfWeek(this DateTime super, int week = -1)
        {
            int weekYear = ISOWeek.GetYear(super);
            if (week == -1)
            {
                week = ISOWeek.GetWeekOfYear(super);
            }
            else if (week < 1 || week > ISOWeek.GetWeeksInYear(weekYear))
            {
                throw new ArgumentOutOfRangeException(nameof(week));
            }

            return ISOWeek.ToDateTime(weekYear, week, DayOfWeek.Monday);
        }

        /// <summary>Returns the Monday of the ISO week before the date's week.</summary>
        /// <param name="super">The source date and time.</param>
        public static DateTime GetPreviousWeek(this DateTime super) => super.GetFirstDateOfWeek().AddDays(-NUMBER_OF_DAYS_IN_A_WEEK);

        /// <summary>Returns the Monday of the ISO week after the date's week.</summary>
        /// <param name="super">The source date and time.</param>
        public static DateTime GetNextWeek(this DateTime super) => super.GetFirstDateOfWeek().AddDays(NUMBER_OF_DAYS_IN_A_WEEK);

        /// <summary>Adjusts the hour to the given bounds while retaining the minute, second, and fractional second.</summary>
        /// <param name="super">The source date and time.</param>
        /// <param name="minHour">The minimum hour.</param>
        /// <param name="maxHour">The maximum hour.</param>
        public static DateTime HourCapped(this DateTime super, int minHour, int maxHour)
        {
            int n = minHour - super.Hour;
            if (n > 0)
            {
                return super.AddHours(n);
            }
            n = super.Hour - maxHour;
            return n > 0 ? super.AddHours(-n) : super;
        }

        /// <summary>Returns a date with the same ticks and an unspecified kind.</summary>
        /// <param name="super">The source date and time.</param>
        public static DateTime Clone(this DateTime super) => new(super.Ticks);

        /// <summary>Returns a date one tick later with an unspecified kind.</summary>
        /// <param name="super">The source date and time.</param>
        /// <exception cref="ArgumentOutOfRangeException">The input is <see cref="DateTime.MaxValue"/>.</exception>
        public static DateTime Similar(this DateTime super)
        {
            long ticks = super.Ticks;
            long i = ticks - (10 * (ticks / 10));
            long r = ticks - i;
            ticks = r == 9 ? i : i + r + 1;
            return new(ticks);
        }

        /// <summary>Formats a date and time including milliseconds using the current culture.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToDateTimeString(this DateTime super) => super.ToString(FORMAT_DATE_TIME);

        /// <summary>Formats a date and time through seconds using the current culture.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToDateSecondString(this DateTime super) => super.ToString(FORMAT_DATE_SECOND);

        /// <summary>Formats a date and time through minutes using the current culture.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToDateHourString(this DateTime super) => super.ToString(FORMAT_DATE_HOUR);

        /// <summary>Formats a full date using the current culture.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToDateDayString(this DateTime super) => super.ToString(FORMAT_DATE_DAY);

        /// <summary>Formats a year and month using the current culture.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToDateMonthString(this DateTime super) => super.ToString(FORMAT_DATE_MONTH);

        /// <summary>Formats a year using the current culture.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToDateYearString(this DateTime super) => super.ToString(FORMAT_DATE_YEAR);

        /// <summary>Formats the time through minutes using the current culture.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToHourString(this DateTime super) => super.ToString(FORMAT_HOUR);

        /// <summary>Formats the abbreviated weekday name using the current culture.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToDayOfWeekShort(this DateTime super) => super.ToString(FORMAT_DAY_OF_WEEK_SHORT);

        /// <summary>Formats the full weekday name using the current culture.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToDayOfWeekLong(this DateTime super) => super.ToString(FORMAT_DAY_OF_WEEK_LONG);

        /// <summary>Formats the day of the month followed by a space.</summary>
        /// <param name="super">The source date and time.</param>
        public static string ToDayOfMonthNumber(this DateTime super) => super.ToString(FORMAT_DAY_OF_MONTH_NUMBER);
    }
}
