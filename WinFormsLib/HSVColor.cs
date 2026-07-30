namespace WinFormsLib
{
    public class HSVColor(double hue, double saturation, double value)
    {
        public double Hue { get; set; } = ((hue % 360d) + 360d) % 360d;
        public double Saturation { get; set; } = Math.Clamp(saturation, 0d, 1d);
        public double Value { get; set; } = Math.Clamp(value, 0d, 1d);

        public Color ToColor()
        {
            double hue = ((Hue % 360d) + 360d) % 360d;
            double saturation = Math.Clamp(Saturation, 0d, 1d);
            double value = Math.Clamp(Value, 0d, 1d);

            if (saturation == 0d)
            {
                int gray = (int)Math.Round(value * 255d);
                return Color.FromArgb(gray, gray, gray);
            }

            double sectorPosition = hue / 60d;
            int sector = (int)Math.Floor(sectorPosition);
            double fraction = sectorPosition - sector;

            double p = value * (1d - saturation);
            double q = value * (1d - (saturation * fraction));
            double t = value * (1d - (saturation * (1d - fraction)));

            (double red, double green, double blue) = sector switch
            {
                0 => (value, t, p),
                1 => (q, value, p),
                2 => (p, value, t),
                3 => (p, q, value),
                4 => (t, p, value),
                _ => (value, p, q)
            };

            return Color.FromArgb(
                (int)Math.Round(red * 255d),
                (int)Math.Round(green * 255d),
                (int)Math.Round(blue * 255d));
        }
    }
}
