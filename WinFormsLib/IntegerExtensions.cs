namespace WinFormsLib
{
    public static partial class IntegerExtensions
    {
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
