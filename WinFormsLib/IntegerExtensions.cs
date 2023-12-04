namespace WinFormsLib
{
    public static partial class IntegerExtensions
    {
        public static string ToDigits(this int super, int minLength = 1)
        {
            string s = super.ToString();
            return $"{Chars.ZERO * Math.Max(0, minLength - s.Length)}{s}";
        }
    }
}
