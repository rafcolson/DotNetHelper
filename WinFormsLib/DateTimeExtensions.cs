using System.Text.Json;
using System.Globalization;
using System.Text.Json.Serialization;

using static WinFormsLib.Constants;

namespace WinFormsLib
{
    public static class DateTimeExtensions
    {
        public const string FORMAT_DATE_TIME = "yyyy-MM-dd HH.mm.ss fff";
        public const string FORMAT_DATE_SECOND = "yyyy-MM-dd HH.mm.ss";
        public const string FORMAT_DATE_HOUR = "yyyy-MM-dd HH.mm";
        public const string FORMAT_DATE_DAY = "yyyy-MM-dd";
        public const string FORMAT_DATE_MONTH = "yyyy-MM";
        public const string FORMAT_DATE_YEAR = "yyyy";

        public const string FORMAT_HOUR = "HH:mm";
        public const string FORMAT_DAY_OF_WEEK_SHORT = "ddd";
        public const string FORMAT_DAY_OF_WEEK_LONG = "dddd";
        public const string FORMAT_DAY_OF_MONTH_NUMBER = "d ";

        public const int NUMBER_OF_WEEKS_IN_A_YEAR = 52;
        public const int NUMBER_OF_DAYS_IN_A_WEEK = 7;

        public static readonly JsonConverter DateTimeJsonConverter = new DateTime_JsonConverter();
        public static readonly JsonConverter DateSecondJsonConverter = new DateSecond_JsonConverter();
        public static readonly JsonConverter DateHourJsonConverter = new DateHour_JsonConverter();
        public static readonly JsonConverter DateDayJsonConverter = new DateDay_JsonConverter();
        public static readonly JsonConverter DateMonthJsonConverter = new DateMonth_JsonConverter();
        public static readonly JsonConverter DateYearJsonConverter = new DateYear_JsonConverter();

        public enum DateTimeFormat
        {
            [Utils.Value(FORMAT_DATE_TIME)]
            Time = 0,
            [Utils.Value(FORMAT_DATE_SECOND)]
            Second = 1,
            [Utils.Value(FORMAT_DATE_HOUR)]
            Hour = 2,
            [Utils.Value(FORMAT_DATE_DAY)]
            Day = 3,
            [Utils.Value(FORMAT_DATE_MONTH)]
            Month = 4,
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

        public static bool IsLeapYear(this DateTime super) => DateTime.IsLeapYear(super.Year);

        public static int GetDaysInYear(this DateTime super) => CULTURE_INFO_DEFAULT.Calendar.GetDaysInYear(super.Year);

        public static int GetDaysInMonth(this DateTime super) => DateTime.DaysInMonth(super.Year, super.Month);

        public static int GetDayOfWeek(this DateTime super, bool firstDaySunday = false)
        {
            int i = (int)super.DayOfWeek;
            if (!firstDaySunday)
            {
                i = i != 0 ? i - 1 : 6;
            }
            return i;
        }

        private static int GetWeeksInYear(int year)
        {
            DateTime jan1 = new(year, 1, 1);
            int n = jan1.DayOfWeek == DayOfWeek.Thursday ? NUMBER_OF_WEEKS_IN_A_YEAR + 1 : NUMBER_OF_WEEKS_IN_A_YEAR;
            if (n == NUMBER_OF_WEEKS_IN_A_YEAR && DateTime.IsLeapYear(year) && jan1.DayOfWeek == DayOfWeek.Wednesday)
            {
                n = NUMBER_OF_WEEKS_IN_A_YEAR + 1;
            }
            return n;
        }

        public static int GetWeeksInYear(this DateTime super) => GetWeeksInYear(super.Year);

        public static int GetWeek(this DateTime super) => CULTURE_INFO_DEFAULT.Calendar.GetWeekOfYear(super, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        private static DateTime GetFirstDateOfWeek(int year, int week)
        {
            DateTime jan1 = new(year, 1, 1);
            DateTime firstThursday = jan1.AddDays(DayOfWeek.Thursday - jan1.DayOfWeek);
            int firstWeek = firstThursday.GetWeek();
            int numWeeks = firstWeek == 1 ? week - 1 : week;
            return firstThursday.AddDays(numWeeks * NUMBER_OF_DAYS_IN_A_WEEK - 3);
        }

        public static DateTime GetFirstDateOfWeek(this DateTime super, int week = -1)
        {
            if (week == -1)
            {
                week = super.GetWeek();
            }
            return GetFirstDateOfWeek(super.Year, week);
        }

        public static DateTime GetPreviousWeek(this DateTime super)
        {
            int year = super.Year;
            int week = super.GetWeek();
            if (week == 1)
            {
                year--;
                week = GetWeeksInYear(year);
            }
            else
            {
                week--;
            }
            return GetFirstDateOfWeek(year, week);
        }

        public static DateTime GetNextWeek(this DateTime super)
        {
            int year = super.Year;
            int week = super.GetWeek();
            int numberOfWeeks = GetWeeksInYear(year);
            if (week == numberOfWeeks)
            {
                week = 1;
                year++;
            }
            else
            {
                week++;
            }
            return GetFirstDateOfWeek(year, week);
        }

        public static DateTime HourCapped(this DateTime super, int minHour, int maxHour)
        {
            int n = minHour - super.Hour;
            if (n > 0)
            {
                return super.AddHours(n);
            }
            n = super.Hour - maxHour;
            if (n > 0)
            {
                return super.AddHours(-n);
            }
            return super;
        }

        public static DateTime Clone(this DateTime super) => new(super.Ticks);

        public static DateTime Similar(this DateTime super)
        {
            long ticks = super.Ticks;
            long i = ticks - 10 * (ticks / 10);
            long r = ticks - i;
            ticks = r == 9 ? i : i + r + 1;
            return new(ticks);
        }

        public static string ToDateTimeString(this DateTime super) => super.ToString(FORMAT_DATE_TIME);

        public static string ToDateSecondString(this DateTime super) => super.ToString(FORMAT_DATE_SECOND);

        public static string ToDateHourString(this DateTime super) => super.ToString(FORMAT_DATE_HOUR);

        public static string ToDateDayString(this DateTime super) => super.ToString(FORMAT_DATE_DAY);

        public static string ToDateMonthString(this DateTime super) => super.ToString(FORMAT_DATE_MONTH);

        public static string ToDateYearString(this DateTime super) => super.ToString(FORMAT_DATE_YEAR);

        public static string ToHourString(this DateTime super) => super.ToString(FORMAT_HOUR);

        public static string ToDayOfWeekShort(this DateTime super) => super.ToString(FORMAT_DAY_OF_WEEK_SHORT);
        
        public static string ToDayOfWeekLong(this DateTime super) => super.ToString(FORMAT_DAY_OF_WEEK_LONG);
        
        public static string ToDayOfMonthNumber(this DateTime super) => super.ToString(FORMAT_DAY_OF_MONTH_NUMBER);
    }
}
