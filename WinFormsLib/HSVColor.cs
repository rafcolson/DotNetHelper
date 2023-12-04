namespace WinFormsLib
{
    public class HSVColor
    {
        private double _hue;
        private double _saturation;
        private double _value;

        public double Hue { get => _hue; set => _hue = Math.Min(Math.Max(value, 0d), 360d); }
        public double Saturation { get => _saturation; set => _saturation = Math.Min(Math.Max(value, 0d), 1d); }
        public double Value { get => _value; set => _value = Math.Min(Math.Max(value, 0d), 1d); }

        public HSVColor(double hue, double saturation, double value)
        {
            _hue = hue;
            _saturation = saturation;
            _value = value;
        }

        public Color ToColor()
        {
            double s = _saturation;
            double v = _value;
            if (s.Equals(0d))
            {
                int j = (int)Math.Round(v * 255d);
                return Color.FromArgb(j, j, j);
            }
            double h = _hue / 60d;
            int i = (int)Math.Round(Math.Floor(h));
            double f = h - i;
            double p = v * (1d - s);
            double q = v * (1d - s * f);
            double t = v * (1d - s * (1d - f));
            double r;
            double g;
            double b;
            switch (i)
            {
                case 0:
                    {
                        r = v * 255d;
                        g = t * 255d;
                        b = p * 255d;
                        break;
                    }

                case 1:
                    {
                        r = q * 255d;
                        g = v * 255d;
                        b = p * 255d;
                        break;
                    }

                case 2:
                    {
                        r = p * 255d;
                        g = v * 255d;
                        b = t * 255d;
                        break;
                    }

                case 3:
                    {
                        r = p * 255d;
                        g = q * 255d;
                        b = v * 255d;
                        break;
                    }

                case 4:
                    {
                        r = t * 255d;
                        g = p * 255d;
                        b = v * 255d;
                        break;
                    }

                default:
                    {
                        r = v * 255d;
                        g = p * 255d;
                        b = q * 255d;
                        break;
                    }
            }
            return Color.FromArgb((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
        }
    }
}
