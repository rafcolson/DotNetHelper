namespace WinFormsLib
{
    public class CMYKColor
    {
        public double C { get; set; }
        public double M { get; set; }
        public double Y { get; set; }
        public double K { get; set; }

        public CMYKColor(double cyan, double magenta, double yellow, double black)
        {
            C = cyan;
            M = magenta;
            Y = yellow;
            K = black;
        }

        public Color ToColor()
        {
            byte r = Convert.ToByte(Math.Min((1d - Math.Min(1d, C * (1d - K) + K)) * 255d, 255));
            byte g = Convert.ToByte(Math.Min((1d - Math.Min(1d, M * (1d - K) + K)) * 255d, 255));
            byte b = Convert.ToByte(Math.Min((1d - Math.Min(1d, Y * (1d - K) + K)) * 255d, 255));
            return Color.FromArgb(r, g, b);
        }
    }
}
