using System.Text.Json;
using System.Globalization;

using static WinFormsLib.Chars;
using static WinFormsLib.Constants;
using static WinFormsLib.DateTimeExtensions;

namespace WinFormsLib
{
    public static class StringExtensions
    {
        public static long GetByteCount(this string super) => System.Text.Encoding.UTF8.GetByteCount(super);

        public static byte[] ToBytes(this string super) => System.Text.Encoding.UTF8.GetBytes(super);

        public static string[] ToLines(this string super) => super.Split(new string[] { LINE_FEED.ToString() }, StringSplitOptions.None);

        public static string ToProper(this string super) => CULTURE_INFO_DEFAULT.TextInfo.ToTitleCase(super);

        public static string ToDigits(this string super)
        {
            string s = string.Empty;
            foreach (char c in super.ToCharArray())
            {
                if (char.IsDigit(c))
                {
                    s += c.ToString();
                }
            }
            return s;
        }

        public static string Strip(this string super, string delimiter)
        {
            string s = string.Empty;
            foreach (char c in super.ToCharArray())
            {
                if (!delimiter.Contains(c))
                {
                    s += c.ToString();
                }
            }
            return s;
        }

        public static string Strip(this string super, char delimiter) => super.Strip(delimiter.ToString());

        public static string[]? SplitFirst(this string super, string delimiter)
        {
            int i = super.IndexOf(delimiter);
            if (i != -1)
            {
                string first = super[..i];
                string last = super[(i + delimiter.Length)..];
                return new string[] { first, last };
            }
            return null;
        }

        public static string[]? SplitFirst(this string super, char delimiter) => super.SplitFirst(delimiter.ToString());

        public static string[]? SplitLast(this string super, string delimiter)
        {
            int i = super.LastIndexOf(delimiter);
            if (i != -1)
            {
                string first = super[..i];
                string last = super[(i + delimiter.Length)..];
                return new string[] { first, last };
            }
            return null;
        }

        public static string[]? SplitLast(this string super, char delimiter) => super.SplitLast(delimiter.ToString());

        public static string ToPascalCase(this string super)
        {
            IEnumerable<string> sa = super.Split
            (
                new[] { HYPHEN, UNDERSCORE },
                StringSplitOptions.RemoveEmptyEntries).Select(x => x[..1].ToUpper() + x[1..].ToLower()
            );
            return string.Concat(sa);
        }

        public static bool IsNumber(this string super) => double.TryParse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT, out _);

        public static bool IsWholeNumber(this string super) => long.TryParse(super, out _);

        public static bool IsFloatingPointNumber(this string super) => !super.IsWholeNumber();

        public static bool IsBoolean(this string super) => bool.TryParse(super, out _);

        public static bool IsInteger(this string super) => int.TryParse(super, out _);

        public static bool IsLong(this string super) => long.TryParse(super, out _) & !super.IsInteger();

        public static bool IsSingle(this string super) => float.TryParse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT, out _);

        public static bool IsDouble(this string super) => double.TryParse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT, out _) & !super.IsSingle();

        public static bool IsColorHtml(this string super) => !super.AsHtmlColor().Equals(Color.Empty);

        public static bool IsColorHex(this string super) => int.TryParse(super.TrimStart(NUMBER_SIGN), NumberStyles.HexNumber, CULTURE_INFO_DEFAULT, out _);

        public static bool IsKnownColor(this string super) => ColorExtensions.KnownColorNames.Contains(super);

        public static bool IsKnownColorArgb(this string super) => ColorExtensions.KnownColorArgbValues.Contains(super.TrimStart(NUMBER_SIGN).AsInt());

        public static bool IsString(this string super)
        {
            int n = super.Length;
            return n > 1 && super[0] == DOUBLE_QUOTE && super[n - 1] == DOUBLE_QUOTE;
        }

        public static bool IsArray(this string super)
        {
            int n = super.Length;
            return n > 1 && super[0] == LEFT_SQUARE_BRACKET && super[n - 1] == RIGHT_SQUARE_BRACKET;
        }

        public static bool IsDictionary(this string super)
        {
            int n = super.Length;
            return n > 1 && super[0] == LEFT_CURLY_BRACE && super[n - 1] == RIGHT_CURLY_BRACE;
        }

        public static FontStyle AsFontStyle(this string super)
        {
            FontStyle fs = new();
            fs.FromString(super);
            return fs;
        }

        public static object? AsObject(this string super)
        {
            if (super.Equals("null"))
            {
                return null;
            }
            else if (super.IsString())
            {
                return super.AsString();
            }
            else if (super.IsArray())
            {
                return super.AsArray();
            }
            else if (super.IsDictionary())
            {
                return super.AsDictionary();
            }
            else if (bool.TryParse(super, out bool b))
            {
                return b;
            }
            else if (long.TryParse(super, out long l))
            {
                return int.TryParse(super, out int i) ? i : (object)l;
            }
            else if (double.TryParse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT, out double d))
            {
                return float.TryParse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT, out float f) ? f : (object)d;
            }
            else if (int.TryParse(super.TrimStart(NUMBER_SIGN), NumberStyles.HexNumber, CULTURE_INFO_DEFAULT, out int argb))
            {
                return Color.FromArgb(argb);
            }
            else if (super.IsKnownColor())
            {
                return super.AsKnownColor();
            }
            else if (super.AsHtmlColor() is Color clr && clr != Color.Empty)
            {
                return clr;
            }
            else if (super.AsDateTime() is DateTime dt)
            {
                return dt;
            }
            return super;
        }

        public static string AsString(this string super) => super.Trim(DOUBLE_QUOTE);

        public static bool AsBoolean(this string super) => bool.Parse(super);

        public static int AsInt(this string super) => int.Parse(super);

        public static long AsLong(this string super) => long.Parse(super);

        public static float AsSingle(this string super) => float.Parse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT);

        public static double AsDouble(this string super) => double.Parse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT);

        public static Color AsHtmlColor(this string super) => ColorTranslator.FromHtml(super);

        public static Color AsHexColor(this string super)
        {
            return int.TryParse(super.TrimStart(NUMBER_SIGN), NumberStyles.HexNumber, CULTURE_INFO_DEFAULT, out int argb) ? Color.FromArgb(argb) : Color.Empty;
        }

        public static Color AsArgbColor(this string super) => Color.FromArgb(super.AsInt());

        public static Color AsKnownColor(this string super) => Color.FromName(super);

        public static DateTime? AsDateTime(this string super, DateTimeFormat formatting = DateTimeFormat.Time)
        {
            string? pattern = formatting.GetValue<string>();
            if (DateTime.TryParseExact(super, pattern, null, DateTimeStyles.None, out DateTime dte))
            {
                return dte;
            }
            else if (DateTime.TryParse(super, out DateTime dt))
            {
                return dt;
            }
            return null;
        }

        public static object[]? AsArray(this string super) => JsonSerializer.Deserialize<object[]>(super);

        public static string[]? AsStringArray(this string super) => JsonSerializer.Deserialize<string[]>(super);

        public static bool[]? AsBooleanArray(this string super) => JsonSerializer.Deserialize<bool[]>(super);

        public static int[]? AsIntArray(this string super) => JsonSerializer.Deserialize<int[]>(super);

        public static long[]? AsLongArray(this string super) => JsonSerializer.Deserialize<long[]>(super);

        public static float[]? AsFloatArray(this string super) => JsonSerializer.Deserialize<float[]>(super);

        public static double[]? AsDoubleArray(this string super) => JsonSerializer.Deserialize<double[]>(super);

        public static Dictionary<string, object?>? AsDictionary(this string super) => JsonSerializer.Deserialize<Dictionary<string, object?>>(super);

        public static Dictionary<string, string>? AsStringDictionary(this string super) => JsonSerializer.Deserialize<Dictionary<string, string>>(super);

        public static T? AsEnumFromGlobal<T>(this string super) where T : Enum
        {
            foreach (T t in Enum.GetValues(typeof(T)))
            {
                if (super == t.GetGlobalStringValue())
                {
                    return t;
                }
            }
            return default;
        }
    }
}
