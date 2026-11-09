namespace WinFormsLib
{
    /// <summary>Provides formatting helpers for integers.</summary>
    public static partial class IntegerExtensions
    {
        /// <summary>Formats an integer with leading zeros as needed.</summary>
        /// <param name="value">The integer to format.</param>
        /// <param name="minLength">The minimum length, including the minus sign for a negative value.</param>
        /// <returns>The formatted integer.</returns>
        public static string ToDigits(this int value, int minLength = 1)
        {
            bool negative = value < 0;
            string digits = Math.Abs((long)value).ToString();

            int digitLength = negative ? minLength - 1 : minLength;
            digits = digits.PadLeft(Math.Max(digits.Length, digitLength), Chars.ZERO);

            return negative ? Chars.HYPHEN + digits : digits;
        }
    }
}
