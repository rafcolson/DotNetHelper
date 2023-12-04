using static WinFormsLib.Chars;

namespace WinFormsLib
{
    public static class ArrayExtensions
    {
        public static Icon ToIcon(this byte[] super)
        {
            using MemoryStream ms = new(super);
            return new(ms);
        }

        public static void Export(this byte[] super, string filePath)
        {
            using BinaryWriter binaryWriter = new(new FileStream(filePath, FileMode.OpenOrCreate));
            binaryWriter.Write(super);
        }

        public static object? TryNext(this object[] super, object o)
        {
            return !super.Any() ? null : super[Math.Min(Array.IndexOf(super, o) + 1, super.Length - 1)];
        }

        public static object? TryPrevious(this object[] super, object o)
        {
            return !super.Any() ? null : super[Math.Max(Array.IndexOf(super, o) - 1, 0)];
        }

        public static object? TryClosest(this object[] super, object o)
        {
            object? _out = super.TryPrevious(o);
            _out ??= super.TryNext(o);
            return _out;
        }

        public static string NextNumerable(this string[] super, string s)
        {
            while ((super.Contains(s)))
            {
                string d = s[^1..].ToDigits();
                if (d.Any())
                {
                    int i = int.Parse(d);
                    int j = 0;
                    while ((!j.Equals(i)))
                        j += 1;
                    j += 1;
                    s = s[0..^1] + j.ToString();
                }
                else
                    s += ZERO;
            }
            return s;
        }

        public static string ToJson(this object[] super, bool includeBrackets)
        {
            string s = super.ToJson();
            if (includeBrackets)
            {
                return s;
            }
            return s.Trim(new char[] { LEFT_SQUARE_BRACKET, RIGHT_SQUARE_BRACKET });
        }
    }
}
