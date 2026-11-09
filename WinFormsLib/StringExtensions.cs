using Microsoft.VisualBasic.CompilerServices;
using System.Globalization;
using System.Text.Json;
using static WinFormsLib.Chars;
using static WinFormsLib.Constants;
using static WinFormsLib.DateTimeExtensions;

namespace WinFormsLib
{
    /// <summary>Provides UTF-8 encoding, text operations, and conversions from strings.</summary>
    public static class StringExtensions
    {
        /// <summary>Returns the number of bytes needed to encode the text as UTF-8.</summary>
        /// <param name="super">The source text.</param>
        public static long GetByteCount(this string super) => System.Text.Encoding.UTF8.GetByteCount(super);

        /// <summary>Encodes the text as UTF-8 without adding a byte order mark.</summary>
        /// <param name="super">The source text.</param>
        public static byte[] ToBytes(this string super) => System.Text.Encoding.UTF8.GetBytes(super);

        /// <summary>Splits on line-feed characters, preserving empty entries and any carriage returns.</summary>
        /// <param name="super">The source text.</param>
        public static string[] ToLines(this string super) => super.Split([LINE_FEED.ToString()], StringSplitOptions.None);

        /// <summary>Applies invariant-culture title casing.</summary>
        /// <param name="super">The source text.</param>
        public static string ToProper(this string super) => CULTURE_INFO_DEFAULT.TextInfo.ToTitleCase(super);

        /// <summary>Keeps only characters recognized as decimal digits.</summary>
        /// <param name="super">The source text.</param>
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

        /// <summary>Removes every character found in the supplied delimiter string.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="delimiter">The separator or characters to match.</param>
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

        /// <summary>Removes every occurrence of the supplied character.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="delimiter">The separator or characters to match.</param>
        public static string Strip(this string super, char delimiter) => super.Strip(delimiter.ToString());

        /// <summary>Splits the text at the first occurrence of the delimiter.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="delimiter">The separator or characters to match.</param>
        /// <returns>The text before and after the delimiter, or null if it is absent.</returns>
        public static string[]? SplitFirst(this string super, string delimiter)
        {
            int i = super.IndexOf(delimiter);
            if (i != -1)
            {
                string first = super[..i];
                string last = super[(i + delimiter.Length)..];
                return [first, last];
            }
            return null;
        }

        /// <summary>Splits the text at the first occurrence of the delimiter.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="delimiter">The separator or characters to match.</param>
        /// <returns>The text before and after the delimiter, or null if it is absent.</returns>
        public static string[]? SplitFirst(this string super, char delimiter) => super.SplitFirst(delimiter.ToString());

        /// <summary>Splits the text at the last occurrence of the delimiter.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="delimiter">The separator or characters to match.</param>
        /// <returns>The text before and after the delimiter, or null if it is absent.</returns>
        public static string[]? SplitLast(this string super, string delimiter)
        {
            int i = super.LastIndexOf(delimiter);
            if (i != -1)
            {
                string first = super[..i];
                string last = super[(i + delimiter.Length)..];
                return [first, last];
            }
            return null;
        }

        /// <summary>Splits the text at the last occurrence of the delimiter.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="delimiter">The separator or characters to match.</param>
        /// <returns>The text before and after the delimiter, or null if it is absent.</returns>
        public static string[]? SplitLast(this string super, char delimiter) => super.SplitLast(delimiter.ToString());

        /// <summary>Removes the first character if it occurs in the supplied delimiter characters.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="delimiter">The separator or characters to match.</param>
        public static string TrimmedFirst(this string super, string delimiter)
        {
            if (super.Length != 0)
            {
                if (delimiter.Contains(super.First()))
                {
                    return super[1..];
                }
            }
            return super;
        }

        /// <summary>Removes the last character if it occurs in the supplied delimiter characters.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="delimiter">The separator or characters to match.</param>
        public static string TrimmedLast(this string super, string delimiter)
        {
            if (super.Length != 0)
            {
                if (delimiter.Contains(super.Last()))
                {
                    return super[..^1];
                }
            }
            return super;
        }

        /// <summary>Removes at most one matching character from each end.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="delimiter">The separator or characters to match.</param>
        public static string Trimmed(this string super, string delimiter) => super.TrimmedFirst(delimiter).TrimmedLast(delimiter);

        /// <summary>Splits on hyphens and underscores and joins the segments after capitalizing their first letters and lowercasing the rest.</summary>
        /// <param name="super">The source text.</param>
        public static string ToPascalCase(this string super)
        {
            IEnumerable<string> sa = super.Split
            (
                [HYPHEN, UNDERSCORE],
                StringSplitOptions.RemoveEmptyEntries).Select(x => x[..1].ToUpper() + x[1..].ToLower()
            );
            return string.Concat(sa);
        }

        /// <summary>Checks whether the text parses as a number using invariant culture.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsNumber(this string super) => double.TryParse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT, out _);

        /// <summary>Checks whether the text parses as a signed 64-bit integer using the current culture.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsWholeNumber(this string super) => long.TryParse(super, out _);

        /// <summary>Checks whether the text parses as a number but not as a signed 64-bit integer.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsFloatingPointNumber(this string super) => super.IsNumber() && !super.IsWholeNumber();

        /// <summary>Checks whether the text parses as a Boolean.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsBoolean(this string super) => bool.TryParse(super, out _);

        /// <summary>Checks whether the text parses as a signed 32-bit integer using the current culture.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsInteger(this string super) => int.TryParse(super, out _);

        /// <summary>Checks whether the text fits a signed 64-bit integer but does not fit a signed 32-bit integer.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsLong(this string super) => long.TryParse(super, out _) & !super.IsInteger();

        /// <summary>Checks whether the text parses as a single-precision number using invariant culture.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsSingle(this string super) => float.TryParse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT, out _);

        /// <summary>Checks whether the text parses as a double but does not parse as a single using invariant culture.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsDouble(this string super) => double.TryParse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT, out _) & !super.IsSingle();

        /// <summary>Checks whether HTML color conversion produces a nonempty color.</summary>
        /// <param name="super">The source text.</param>
        /// <remarks>Invalid HTML color text can cause conversion to throw.</remarks>
        public static bool IsColorHtml(this string super) => !super.AsHtmlColor().Equals(Color.Empty);

        /// <summary>Checks whether the text, after removing leading number signs, parses as a hexadecimal ARGB integer.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsColorHex(this string super) => int.TryParse(super.TrimStart(NUMBER_SIGN), NumberStyles.HexNumber, CULTURE_INFO_DEFAULT, out _);

        /// <summary>Checks for an exact match in the list of known color names.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsKnownColor(this string super) => ColorExtensions.KnownColorNames.Contains(super);

        /// <summary>Parses the text as a decimal ARGB value after removing leading number signs and checks the known-color palette.</summary>
        /// <param name="super">The source text.</param>
        public static bool IsKnownColorArgb(this string super) => ColorExtensions.KnownColorArgbValues.Contains(super.TrimStart(NUMBER_SIGN).AsInt());

        /// <summary>Checks whether the text starts and ends with double quotes.</summary>
        /// <param name="super">The source text.</param>
        /// <remarks>Checks the delimiters only.</remarks>
        public static bool IsString(this string super)
        {
            int n = super.Length;
            return n > 1 && super[0] == DOUBLE_QUOTE && super[n - 1] == DOUBLE_QUOTE;
        }

        /// <summary>Checks whether the text starts and ends with square brackets.</summary>
        /// <param name="super">The source text.</param>
        /// <remarks>Checks the delimiters only.</remarks>
        public static bool IsArray(this string super)
        {
            int n = super.Length;
            return n > 1 && super[0] == LEFT_SQUARE_BRACKET && super[n - 1] == RIGHT_SQUARE_BRACKET;
        }

        /// <summary>Checks whether the text starts and ends with braces.</summary>
        /// <param name="super">The source text.</param>
        /// <remarks>Checks the delimiters only.</remarks>
        public static bool IsDictionary(this string super)
        {
            int n = super.Length;
            return n > 1 && super[0] == LEFT_CURLY_BRACE && super[n - 1] == RIGHT_CURLY_BRACE;
        }

        /// <summary>Converts comma-separated style names to font style flags.</summary>
        /// <param name="super">The source text.</param>
        public static FontStyle AsFontStyle(this string super)
        {
            FontStyle fs = new();
            fs.FromString(super);
            return fs;
        }

        /// <summary>Interprets the text as a null value, string, collection, scalar, color, or date using the library's conversion rules.</summary>
        /// <param name="super">The source text.</param>
        /// <remarks>If no conversion matches, the source text is returned. Conversion exceptions can propagate.</remarks>
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

        /// <summary>Removes double quotes from both ends without interpreting escape sequences.</summary>
        /// <param name="super">The source text.</param>
        public static string AsString(this string super) => super.Trim(DOUBLE_QUOTE);

        /// <summary>Parses the text as a Boolean.</summary>
        /// <param name="super">The source text.</param>
        public static bool AsBoolean(this string super) => bool.Parse(super);

        /// <summary>Parses a signed 32-bit integer using the current culture.</summary>
        /// <param name="super">The source text.</param>
        public static int AsInt(this string super) => int.Parse(super);

        /// <summary>Parses a signed 64-bit integer using the current culture.</summary>
        /// <param name="super">The source text.</param>
        public static long AsLong(this string super) => long.Parse(super);

        /// <summary>Parses a single-precision number using invariant culture.</summary>
        /// <param name="super">The source text.</param>
        public static float AsSingle(this string super) => float.Parse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT);

        /// <summary>Parses a double-precision number using invariant culture.</summary>
        /// <param name="super">The source text.</param>
        public static double AsDouble(this string super) => double.Parse(super, NumberStyles.Number, CULTURE_INFO_DEFAULT);

        /// <summary>Converts HTML color text to a color.</summary>
        /// <param name="super">The source text.</param>
        public static Color AsHtmlColor(this string super) => ColorTranslator.FromHtml(super);

        /// <summary>Parses a hexadecimal ARGB value, returning an empty color if parsing fails.</summary>
        /// <param name="super">The source text.</param>
        public static Color AsHexColor(this string super)
        {
            return int.TryParse(super.TrimStart(NUMBER_SIGN), NumberStyles.HexNumber, CULTURE_INFO_DEFAULT, out int argb) ? Color.FromArgb(argb) : Color.Empty;
        }

        /// <summary>Parses a decimal ARGB value and converts it to a color.</summary>
        /// <param name="super">The source text.</param>
        public static Color AsArgbColor(this string super) => Color.FromArgb(super.AsInt());

        /// <summary>Creates a color from the supplied name.</summary>
        /// <param name="super">The source text.</param>
        public static Color AsKnownColor(this string super) => Color.FromName(super);

        /// <summary>Tries the selected date format first, then general date parsing.</summary>
        /// <param name="super">The source text.</param>
        /// <param name="formatting">The date format to try first.</param>
        /// <returns>The parsed date and time, or null if parsing fails.</returns>
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

        /// <summary>Deserializes a JSON array into object values.</summary>
        /// <param name="super">The source text.</param>
        public static object[]? AsArray(this string super) => JsonSerializer.Deserialize<object[]>(super);

        /// <summary>Deserializes a JSON array of strings.</summary>
        /// <param name="super">The source text.</param>
        public static string[]? AsStringArray(this string super) => JsonSerializer.Deserialize<string[]>(super);

        /// <summary>Deserializes a JSON array of Boolean values.</summary>
        /// <param name="super">The source text.</param>
        public static bool[]? AsBooleanArray(this string super) => JsonSerializer.Deserialize<bool[]>(super);

        /// <summary>Deserializes a JSON array of signed 32-bit integers.</summary>
        /// <param name="super">The source text.</param>
        public static int[]? AsIntArray(this string super) => JsonSerializer.Deserialize<int[]>(super);

        /// <summary>Deserializes a JSON array of signed 64-bit integers.</summary>
        /// <param name="super">The source text.</param>
        public static long[]? AsLongArray(this string super) => JsonSerializer.Deserialize<long[]>(super);

        /// <summary>Deserializes a JSON array of single-precision numbers.</summary>
        /// <param name="super">The source text.</param>
        public static float[]? AsFloatArray(this string super) => JsonSerializer.Deserialize<float[]>(super);

        /// <summary>Deserializes a JSON array of double-precision numbers.</summary>
        /// <param name="super">The source text.</param>
        public static double[]? AsDoubleArray(this string super) => JsonSerializer.Deserialize<double[]>(super);

        /// <summary>Deserializes a JSON object into a dictionary with object values.</summary>
        /// <param name="super">The source text.</param>
        public static Dictionary<string, object?>? AsDictionary(this string super) => JsonSerializer.Deserialize<Dictionary<string, object?>>(super);

        /// <summary>Deserializes a JSON object into a dictionary with string values.</summary>
        /// <param name="super">The source text.</param>
        public static Dictionary<string, string>? AsStringDictionary(this string super) => JsonSerializer.Deserialize<Dictionary<string, string>>(super);

        /// <summary>Builds a map from a serialized array of colon-separated key-value strings.</summary>
        /// <param name="super">The source text.</param>
        /// <remarks>Entries that fail to convert, duplicate keys, and null values are skipped.</remarks>
        public static Map<string, object> AsMap(this string super)
        {
            if (super.Trimmed(Conversions.ToString(LEFT_CURLY_BRACE) + RIGHT_CURLY_BRACE).AsStringArray() is not string[] sa)
            {
                return [];
            }
            Map<string, object> d = [];
            foreach (string s in sa)
            {
                try
                {
                    if (s.SplitFirst(Conversions.ToString(COLON)) is string[] it)
                    {
                        if (it.Last().TrimStart(SPACE).AsObject() is object o)
                        {
                            d.Add(it.First().AsString(), o);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return d;
        }

        /// <summary>Deserializes a JSON object whose string values contain serialized maps.</summary>
        /// <param name="super">The source text.</param>
        public static Map<string, Map<string, object>> AsNestedMap(this string super)
        {
            if (super.AsStringDictionary() is not Dictionary<string, string> sd)
            {
                return [];
            }
            Map<string, Map<string, object>> d = [];
            foreach (KeyValuePair<string, string> kvp in sd)
            {
                d.Add(kvp.Key, kvp.Value.AsMap());
            }
            return d;
        }

        /// <summary>Finds the first enum member whose global string equals the supplied text.</summary>
        /// <typeparam name="T">The enum type to search.</typeparam>
        /// <param name="super">The source text.</param>
        /// <returns>The matching enum member, or null if none matches.</returns>
        public static T? AsEnumFromGlobal<T>(this string super) where T : struct, Enum
        {
            foreach (T t in Enum.GetValues<T>())
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
