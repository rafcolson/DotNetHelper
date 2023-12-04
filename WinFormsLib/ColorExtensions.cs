using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    public static class ColorExtensions
    {
        private class ColorJsonConverter : JsonConverter<Color>
        {
            public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s) ? s.AsArgbColor() : Color.Empty;
            }
            public override void Write(Utf8JsonWriter writer, Color color, JsonSerializerOptions options)
            {
                writer.WriteStringValue(color.ToArgb().ToString());
            }
        }
        public static JsonConverter JsonConverter => new ColorJsonConverter();

        public static readonly string[] KnownColorNames = GetKnownColorNames();
        public static readonly int[] GrayColorArgbValues = GetArgbValues(GetGrayColors());
        public static readonly int[] BrightColorArgbValues = GetArgbValues(GetBrightColors());
        public static readonly int[] HueColorArgbValues = GetArgbValues(GetHueColors());
        public static readonly int[] BasicColorArgbValues = GetArgbValues(GetBasicColors());
        public static readonly int[] KnownColorArgbValues = GetArgbValues(GetKnownColors());
        public static readonly int[] ColorArgbValues = GetArgbValues(GetColors());

        private static Color[] GetColors()
        {
            List<Color> colors = new();
            colors.AddRange(GetBrightColors());
            colors.AddRange(GetHueColors());
            colors.AddRange(GetBasicColors());
            List<HSVColor> hsvColors = new();
            foreach (Color c in new HashSet<Color>(colors))
            {
                hsvColors.Add(c.ToHSVColor());
            }
            hsvColors.Sort((hsv1, hsv2) => (hsv1.Hue).CompareTo(hsv2.Hue));
            colors.Clear();
            foreach (HSVColor hsv in hsvColors)
            {
                colors.Add(hsv.ToColor());
            }
            return colors.ToArray();
        }

        private static string[] GetKnownColorNames()
        {
            List<string> colorNames = new();
            foreach (string n in Enum.GetNames<KnownColor>())
            {
                colorNames.Add(n);
            }
            return colorNames.ToArray();
        }

        private static Color[] GetKnownColors()
        {
            List<KnownColor> knownColors = Enum.GetValues<KnownColor>().ToList();
            HashSet<Color> colors = new();
            foreach (KnownColor knowColor in knownColors)
            {
                colors.Add(Color.FromKnownColor(knowColor));
            }
            colors.Remove(Color.Black);
            colors.Remove(Color.White);
            colors.Remove(Color.Transparent);
            List<HSVColor> hsvColors = new();
            foreach (Color c in colors)
            {
                hsvColors.Add(c.ToHSVColor());
            }
            hsvColors.Sort((hsv1, hsv2) => (hsv1.Hue).CompareTo(hsv2.Hue));
            colors.Clear();
            foreach (HSVColor hsv in hsvColors)
            {
                colors.Add(hsv.ToColor());
            }
            return colors.ToArray();
        }

        private static Color[] GetBrightColors()
        {
            List<Color> colors = new()
            {
                new CMYKColor(0, 1, 1, 0).ToColor(),
                new CMYKColor(0, 0.6, 1, 0).ToColor(),
                new CMYKColor(0, 0.3, 1, 0).ToColor(),
                new CMYKColor(0, 0.05, 1, 0).ToColor(),
                new CMYKColor(0.05, 1, 0, 0).ToColor(),
                new CMYKColor(1, 0, 1, 0).ToColor(),
                new CMYKColor(1, 0.15, 1, 0).ToColor(),
                new CMYKColor(1, 0.7, 0, 0).ToColor(),
                new CMYKColor(0.6, 1, 0, 0).ToColor(),
                new CMYKColor(0.3, 0, 0, 0).ToColor()
            };
            List<HSVColor> hsvColors = new();
            foreach (Color c in new HashSet<Color>(colors))
            {
                hsvColors.Add(c.ToHSVColor());
            }
            hsvColors.Sort((hsv1, hsv2) => (hsv1.Hue).CompareTo(hsv2.Hue));
            colors.Clear();
            foreach (HSVColor hsv in hsvColors)
            {
                colors.Add(hsv.ToColor());
            }
            return colors.ToArray();
        }

        private static Color[] GetBasicColors()
        {
            List<Color> colors = new()
            {
                new CMYKColor(0, 1, 0, 0).ToColor(), // Magenta
                new CMYKColor(0, 1, 0, 0.5).ToColor(), // Purple
                new CMYKColor(0, 0.45, 0, 0.7).ToColor(), // Violet
                new CMYKColor(1, 1, 0, 0).ToColor(), // Blue
                new CMYKColor(1, 0, 0, 0.5).ToColor(), // Teal
                new CMYKColor(1, 0, 1, 0).ToColor(), // Green
                new CMYKColor(0.5, 0, 1, 0).ToColor(), // Chartreuse
                new CMYKColor(0, 0, 1, 0).ToColor(), // Yellow
                new CMYKColor(0, 0.25, 1, 0).ToColor(), // Amber
                new CMYKColor(0, 0.5, 1, 0).ToColor(), // Orange
                new CMYKColor(0, 0.71, 0.77, 0.11).ToColor(), // Vermillion
                new CMYKColor(0, 1, 1, 0).ToColor() // Red
            };
            List<HSVColor> hsvColors = new();
            foreach (Color c in new HashSet<Color>(colors))
            {
                hsvColors.Add(c.ToHSVColor());
            }
            hsvColors.Sort((hsv1, hsv2) => (hsv1.Hue).CompareTo(hsv2.Hue));
            colors.Clear();
            foreach (HSVColor hsv in hsvColors)
            {
                colors.Add(hsv.ToColor());
            }
            return colors.ToArray();
        }

        public static Color[] GetHueColors(double saturation = 0.375, double value = 0.95)
        {
            List<Color> l = new();
            for (int i = 8; i < 360; i += 8)
            {
                l.Add(new HSVColor(i, saturation, value).ToColor());
            }
            return l.ToArray();
        }

        private static Color[] GetGrayColors()
        {
            List<Color> l = new();
            for (int i = 32; i <= 128; i += 16)
            {
                l.Add(new CMYKColor(0, 0, 0, i / 256d).ToColor());
            }
            return l.ToArray();
        }

        private static int[] GetArgbValues(IEnumerable<Color> colors)
        {
            List<int> l = new();
            foreach (Color c in colors)
            {
                l.Add(c.ToArgb());
            }
            return l.ToArray();
        }

        public static CMYKColor ToCMYKColor(this Color super)
        {
            byte red = super.R;
            byte green = super.G;
            byte blue = super.B;
            double black = Math.Min(1d - red / 255d, Math.Min(1d - green / 255d, 1d - blue / 255d));
            double cyan = (1d - (red / 255d) - black) / (1d - black);
            double magenta = (1d - (green / 255d) - black) / (1d - black);
            double yellow = (1d - (blue / 255d) - black) / (1d - black);
            return new(cyan, magenta, yellow, black);
        }

        public static HSVColor ToHSVColor(this Color super)
        {
            return new(super.GetHue(), super.GetSaturation(), Math.Max(super.R, Math.Max(super.G, super.B)) / 255d);
        }

        public static Color WithValue(this Color super, double d)
        {
            HSVColor hsv = super.ToHSVColor();
            hsv.Value += d;
            return hsv.ToColor();
        }

        public static bool IsLighterThan(this Color super, Color color) => super.ToHSVColor().Value > color.ToHSVColor().Value;

        public static bool IsDarkerThan(this Color super, Color color) => color.IsLighterThan(super);

        public static Color Mixed(this Color super, Color color, double d = 0.5)
        {
            d = Math.Min(Math.Max(d, 0), 1);
            double d0 = 1 - d;
            byte r = (byte)Math.Round(Math.Min((int)Math.Round(super.R * d0 + color.R * d), 255d));
            byte g = (byte)Math.Round(Math.Min((int)Math.Round(super.G * d0 + color.G * d), 255d));
            byte b = (byte)Math.Round(Math.Min((int)Math.Round(super.B * d0 + color.B * d), 255d));
            return Color.FromArgb(r, g, b);
        }

        public static Color Multiplied(this Color super, Color color)
        {
            byte r = (byte)Math.Round(Math.Min((int)Math.Round(super.R * color.R / 255d), 255d));
            byte g = (byte)Math.Round(Math.Min((int)Math.Round(super.G * color.G / 255d), 255d));
            byte b = (byte)Math.Round(Math.Min((int)Math.Round(super.B * color.B / 255d), 255d));
            return Color.FromArgb(r, g, b);
        }

        public static string ToHtml(this Color super) => ColorTranslator.ToHtml(super);

        public static string ToHex(this Color super) => super.ToArgb().ToString("X8");
    }
}
