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
            return super.Length == 0 ? null : super[Math.Min(Array.IndexOf(super, o) + 1, super.Length - 1)];
        }

        public static object? TryPrevious(this object[] super, object o)
        {
            return super.Length == 0 ? null : super[Math.Max(Array.IndexOf(super, o) - 1, 0)];
        }

        public static object? TryClosest(this object[] super, object o)
        {
            object? _out = super.TryPrevious(o);
            _out ??= super.TryNext(o);
            return _out;
        }

        public static string NextNumerable(this string[] super, string s)
        {
            int suffixStart = s.Length;
            while (suffixStart > 0 && char.IsDigit(s[suffixStart - 1]))
            {
                suffixStart--;
            }

            string stem = s[..suffixStart];
            if (suffixStart == s.Length && !super.Contains(s))
            {
                return s;
            }

            int suffix = suffixStart == s.Length ? 0 : int.Parse(s[suffixStart..]) + 1;
            string candidate = stem + suffix;
            while (super.Contains(candidate))
            {
                candidate = stem + ++suffix;
            }
            return candidate;
        }

        public static string ToJson(this object[] super, bool includeBrackets)
        {
            string s = super.ToJson();
            return includeBrackets ? s : s.Trim([LEFT_SQUARE_BRACKET, RIGHT_SQUARE_BRACKET]);
        }
    }
}
