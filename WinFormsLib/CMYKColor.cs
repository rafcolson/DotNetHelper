namespace WinFormsLib
{
    public class CMYKColor(double cyan, double magenta, double yellow, double black)
    {
        public double C { get; set; } = cyan;
        public double M { get; set; } = magenta;
        public double Y { get; set; } = yellow;
        public double K { get; set; } = black;

        public Color ToColor()
        {
            byte r = Convert.ToByte(Math.Min((1d - Math.Min(1d, C * (1d - K) + K)) * 255d, 255));
            byte g = Convert.ToByte(Math.Min((1d - Math.Min(1d, M * (1d - K) + K)) * 255d, 255));
            byte b = Convert.ToByte(Math.Min((1d - Math.Min(1d, Y * (1d - K) + K)) * 255d, 255));
            return Color.FromArgb(r, g, b);
        }
    }
}
