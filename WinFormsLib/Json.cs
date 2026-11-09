using System.Collections;
using System.Globalization;
using System.Text.Json;

namespace WinFormsLib
{

    /// <summary>Provides helpers for formatting and parsing JSON values.</summary>
    public class Json
    {
        /// <summary>Parses an array, dictionary, or scalar according to the shape of the input.</summary>
        /// <param name="s">The text to parse.</param>
        public static object? FromJson(string s)
        {
            string trimmed = s.Trim();
            return trimmed == "null" || trimmed.StartsWith(Chars.DOUBLE_QUOTE)
                ? JsonSerializer.Deserialize<string>(trimmed)
                : trimmed.IndexOfAny(['e', 'E']) >= 0
                && double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
                ? float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out float single)
                    && float.IsFinite(single) ? (object)single : number
                : s.IsArray() ? s.AsArray() : s.IsDictionary() ? s.AsDictionary() : s.AsObject();
        }

        /// <summary>Serializes a value according to its runtime type.</summary>
        /// <param name="o">The value to format.</param>
        public static string ToJson(object? o)
        {
            return o switch
            {
                System.Drawing.Color color => ToJson(color),
                IDictionary dictionary => ToJson(dictionary),
                string text => ToJson(text),
                IEnumerable sequence => ToJson(sequence),
                _ => JsonSerializer.Serialize(o)
            };
        }

        /// <summary>Formats a Boolean as lowercase <c>true</c> or <c>false</c>.</summary>
        /// <param name="b">The Boolean to format.</param>
        public static string ToJson(bool b)
        {
            return JsonSerializer.Serialize(b);
        }

        /// <summary>Serializes a 32-bit integer as a JSON number.</summary>
        /// <param name="i">The integer to format.</param>
        public static string ToJson(int i)
        {
            return JsonSerializer.Serialize(i);
        }

        /// <summary>Serializes a 64-bit integer as a JSON number.</summary>
        /// <param name="l">The integer to format.</param>
        public static string ToJson(long l)
        {
            return JsonSerializer.Serialize(l);
        }

        /// <summary>Serializes a finite single-precision number as a JSON number.</summary>
        /// <param name="s">The number to format.</param>
        public static string ToJson(float s)
        {
            return JsonSerializer.Serialize(s);
        }

        /// <summary>Serializes a finite double-precision number as a JSON number.</summary>
        /// <param name="d">The number to format.</param>
        public static string ToJson(double d)
        {
            return JsonSerializer.Serialize(d);
        }

        /// <summary>Serializes the string as a quoted, escaped JSON string.</summary>
        /// <param name="s">The string to format.</param>
        public static string ToJson(string s)
        {
            return JsonSerializer.Serialize(s);
        }

        /// <summary>Serializes the HTML representation of the color as a JSON string.</summary>
        /// <param name="c">The color to format.</param>
        public static string ToJson(System.Drawing.Color c)
        {
            return ToJson(c.ToHtml());
        }

        /// <summary>Formats the sequence as a bracketed, comma-separated list in enumeration order.</summary>
        /// <param name="ie">The sequence to format.</param>
        public static string ToJson(IEnumerable ie)
        {
            string s = string.Empty;
            foreach (object? o in ie)
            {
                s += ToJson(o) + Chars.COMMA + Chars.SPACE;
            }

            if (!string.IsNullOrEmpty(s))
            {
                s = s[..^2];
            }
            return Chars.LEFT_SQUARE_BRACKET + s + Chars.RIGHT_SQUARE_BRACKET;
        }

        /// <summary>Formats the dictionary as a brace-delimited list of key-value pairs.</summary>
        /// <param name="id">The dictionary to format.</param>
        /// <remarks>Keys are converted to strings using invariant culture and written as JSON property names.</remarks>
        public static string ToJson(IDictionary id)
        {
            string s = string.Empty;
            foreach (DictionaryEntry e in id)
            {
                string key = Convert.ToString(e.Key, CultureInfo.InvariantCulture) ?? string.Empty;
                s += ToJson(key) + Chars.COLON + Chars.SPACE
                    + ToJson(e.Value) + Chars.COMMA + Chars.SPACE;
            }
            if (!string.IsNullOrEmpty(s))
            {
                s = s[..^2];
            }
            return Chars.LEFT_CURLY_BRACE + s + Chars.RIGHT_CURLY_BRACE;
        }

    }

}
