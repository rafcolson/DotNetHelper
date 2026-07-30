using System.Collections;

namespace WinFormsLib
{

    public class Json
    {
        private static readonly System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.GetCultureInfo("en");

        public static object? FromJson(string s)
        {
            return s.IsArray() ? s.AsArray() : s.IsDictionary() ? s.AsDictionary() : s.AsObject();
        }

        public static string? ToJson(object o)
        {
            if (o == null)
            {
                return "Nothing";
            }

            string? result;
            if (o is bool b)
            {
                result = ToJson(b);
            }
            else if (o is int i)
            {
                result = ToJson(i);
            }
            else if (o is long l)
            {
                result = ToJson(l);
            }
            else if (o is float f)
            {
                result = ToJson(f);
            }
            else if (o is double d)
            {
                result = ToJson(d);
            }
            else
            {
                result = o is string s
                    ? ToJson(s)
                    : o is System.Drawing.Color c ? ToJson(c) : o is IDictionary id ? ToJson(id) : o is IEnumerable ie ? ToJson(ie) : o.ToString();
            }

            return result;
        }

        public static string ToJson(bool b)
        {
            return b.ToString().ToLower();
        }

        public static string ToJson(int i)
        {
            return i.ToString();
        }

        public static string ToJson(long l)
        {
            return l.ToString();
        }

        public static string ToJson(float s)
        {
            return s.ToString(Chars.F_UPPER.ToString(), ci);
        }

        public static string ToJson(double d)
        {
            return d.ToString(Chars.F_UPPER.ToString(), ci);
        }

        public static string ToJson(string s)
        {
            return Chars.DOUBLE_QUOTE + s + Chars.DOUBLE_QUOTE;
        }

        public static string ToJson(System.Drawing.Color c)
        {
            return c.ToHtml();
        }

        public static string ToJson(IEnumerable ie)
        {
            string s = string.Empty;
            foreach (object o in ie)
            {
                s += ToJson(o) + Chars.COMMA + Chars.SPACE;
            }

            if (!string.IsNullOrEmpty(s))
            {
                s = s[..^2];
            }
            return Chars.LEFT_SQUARE_BRACKET + s + Chars.RIGHT_SQUARE_BRACKET;
        }

        public static string ToJson(IDictionary id)
        {
            string s = string.Empty;
            foreach (DictionaryEntry e in id)
            {
                s += ToJson(e.Key) + Chars.COLON + Chars.SPACE
                    + ToJson(e.Value ?? "Nothing") + Chars.COMMA + Chars.SPACE;
            }
            if (!string.IsNullOrEmpty(s))
            {
                s = s[..^2];
            }
            return Chars.LEFT_CURLY_BRACE + s + Chars.RIGHT_CURLY_BRACE;
        }

    }

}
