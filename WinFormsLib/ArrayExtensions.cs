using static WinFormsLib.Chars;

namespace WinFormsLib
{
    /// <summary>Provides array operations for binary data, item navigation, and JSON output.</summary>
    public static class ArrayExtensions
    {
        /// <summary>Creates an icon from encoded icon data.</summary>
        /// <param name="super">The bytes of the icon file.</param>
        /// <returns>An icon that the caller must dispose.</returns>
        public static Icon ToIcon(this byte[] super)
        {
            using MemoryStream ms = new(super);
            return new(ms);
        }

        /// <summary>Writes the bytes to a file, replacing any existing contents.</summary>
        /// <param name="super">The bytes to write.</param>
        /// <param name="filePath">The destination file path.</param>
        public static void Export(this byte[] super, string filePath) => File.WriteAllBytes(filePath, super);

        /// <summary>Gets the item after the first occurrence of the supplied item.</summary>
        /// <param name="super">The array to search.</param>
        /// <param name="o">The item to locate.</param>
        /// <returns>The next item, the first item if the supplied item is absent, the last item if already at the end, or <see langword="null"/> if the array is empty.</returns>
        public static object? TryNext(this object[] super, object o)
        {
            return super.Length == 0 ? null : super[Math.Min(Array.IndexOf(super, o) + 1, super.Length - 1)];
        }

        /// <summary>Gets the item before the first occurrence of the supplied item.</summary>
        /// <param name="super">The array to search.</param>
        /// <param name="o">The item to locate.</param>
        /// <returns>The previous item, the first item if the supplied item is absent or already first, or <see langword="null"/> if the array is empty.</returns>
        public static object? TryPrevious(this object[] super, object o)
        {
            return super.Length == 0 ? null : super[Math.Max(Array.IndexOf(super, o) - 1, 0)];
        }

        /// <summary>Gets the result of <see cref="TryPrevious"/> if non-null, or the result of <see cref="TryNext"/> otherwise.</summary>
        /// <param name="super">The array to search.</param>
        /// <param name="o">The item to locate.</param>
        public static object? TryClosest(this object[] super, object o)
        {
            object? _out = super.TryPrevious(o);
            _out ??= super.TryNext(o);
            return _out;
        }

        /// <summary>Finds an unused name by appending or incrementing a numeric suffix.</summary>
        /// <param name="super">The names already in use.</param>
        /// <param name="s">The starting name.</param>
        /// <returns>The starting name if it has no numeric suffix and is unused. Otherwise, the next unused numbered name.</returns>
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

        /// <summary>Serializes the array to JSON and optionally trims square brackets from both ends of the result.</summary>
        /// <param name="super">The array to serialize.</param>
        /// <param name="includeBrackets">Whether to retain the square brackets.</param>
        public static string ToJson(this object[] super, bool includeBrackets)
        {
            string s = super.ToJson();
            return includeBrackets ? s : s.Trim([LEFT_SQUARE_BRACKET, RIGHT_SQUARE_BRACKET]);
        }
    }
}
