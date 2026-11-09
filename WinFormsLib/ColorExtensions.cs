using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    /// <summary>Provides color conversion, blending, and palette helpers.</summary>
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
        /// <summary>Gets a converter that stores a color as a JSON string containing its signed ARGB integer.</summary>
        public static JsonConverter JsonConverter => new ColorJsonConverter();

        /// <summary>Contains the names declared by the KnownColor enumeration.</summary>
        public static readonly string[] KnownColorNames = GetKnownColorNames();
        /// <summary>Contains the ARGB values of the grayscale palette.</summary>
        public static readonly int[] GrayColorArgbValues = GetArgbValues(GetGrayColors());
        /// <summary>Contains the ARGB values of the bright palette.</summary>
        public static readonly int[] BrightColorArgbValues = GetArgbValues(GetBrightColors());
        /// <summary>Contains the ARGB values of the default hue palette.</summary>
        public static readonly int[] HueColorArgbValues = GetArgbValues(GetHueColors());
        /// <summary>Contains the ARGB values of the basic palette.</summary>
        public static readonly int[] BasicColorArgbValues = GetArgbValues(GetBasicColors());
        /// <summary>Contains the ARGB values derived from known colors, excluding black, white, and transparent.</summary>
        public static readonly int[] KnownColorArgbValues = GetArgbValues(GetKnownColors());
        /// <summary>Contains the ARGB values of the combined bright, hue, and basic palettes.</summary>
        public static readonly int[] ColorArgbValues = GetArgbValues(GetColors());

        private static Color[] GetColors()
        {
            List<Color> colors = [.. GetBrightColors(), .. GetHueColors(), .. GetBasicColors()];
            List<HSVColor> hsvColors = [];
            foreach (Color c in new HashSet<Color>(colors))
            {
                hsvColors.Add(c.ToHSVColor());
            }
            hsvColors.Sort((hsv1, hsv2) => hsv1.Hue.CompareTo(hsv2.Hue));
            colors.Clear();
            foreach (HSVColor hsv in hsvColors)
            {
                colors.Add(hsv.ToColor());
            }
            return [.. colors];
        }

        private static string[] GetKnownColorNames()
        {
            List<string> colorNames = [.. Enum.GetNames<KnownColor>()];
            return [.. colorNames];
        }

        private static Color[] GetKnownColors()
        {
            List<KnownColor> knownColors = [.. Enum.GetValues<KnownColor>()];
            HashSet<Color> colors = [];
            foreach (KnownColor knowColor in knownColors)
            {
                _ = colors.Add(Color.FromKnownColor(knowColor));
            }
            _ = colors.Remove(Color.Black);
            _ = colors.Remove(Color.White);
            _ = colors.Remove(Color.Transparent);
            List<HSVColor> hsvColors = [];
            foreach (Color c in colors)
            {
                hsvColors.Add(c.ToHSVColor());
            }
            hsvColors.Sort((hsv1, hsv2) => hsv1.Hue.CompareTo(hsv2.Hue));
            colors.Clear();
            foreach (HSVColor hsv in hsvColors)
            {
                _ = colors.Add(hsv.ToColor());
            }
            return [.. colors];
        }

        private static Color[] GetBrightColors()
        {
            List<Color> colors =
            [
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
            ];
            List<HSVColor> hsvColors = [];
            foreach (Color c in new HashSet<Color>(colors))
            {
                hsvColors.Add(c.ToHSVColor());
            }
            hsvColors.Sort((hsv1, hsv2) => hsv1.Hue.CompareTo(hsv2.Hue));
            colors.Clear();
            foreach (HSVColor hsv in hsvColors)
            {
                colors.Add(hsv.ToColor());
            }
            return [.. colors];
        }

        private static Color[] GetBasicColors()
        {
            List<Color> colors =
            [
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
            ];
            List<HSVColor> hsvColors = [];
            foreach (Color c in new HashSet<Color>(colors))
            {
                hsvColors.Add(c.ToHSVColor());
            }
            hsvColors.Sort((hsv1, hsv2) => hsv1.Hue.CompareTo(hsv2.Hue));
            colors.Clear();
            foreach (HSVColor hsv in hsvColors)
            {
                colors.Add(hsv.ToColor());
            }
            return [.. colors];
        }

        /// <summary>Creates opaque colors at eight-degree intervals from 8 through 352 degrees.</summary>
        /// <param name="saturation">The saturation component, clamped to the range from 0 to 1.</param>
        /// <param name="value">The value component, clamped to the range from 0 to 1.</param>
        public static Color[] GetHueColors(double saturation = 0.375, double value = 0.95)
        {
            List<Color> l = [];
            for (int i = 8; i < 360; i += 8)
            {
                l.Add(new HSVColor(i, saturation, value).ToColor());
            }
            return [.. l];
        }

        private static Color[] GetGrayColors()
        {
            List<Color> l = [];
            for (int i = 32; i <= 128; i += 16)
            {
                l.Add(new CMYKColor(0, 0, 0, i / 256d).ToColor());
            }
            return [.. l];
        }

        private static int[] GetArgbValues(IEnumerable<Color> colors)
        {
            List<int> l = [];
            foreach (Color c in colors)
            {
                l.Add(c.ToArgb());
            }
            return [.. l];
        }

        /// <summary>Converts the RGB channels to CMYK components.</summary>
        /// <param name="super">The source color. Its alpha channel is ignored.</param>
        /// <remarks>Pure RGB black is represented by a black component of 1 and zero cyan, magenta, and yellow.</remarks>
        public static CMYKColor ToCMYKColor(this Color super)
        {
            byte red = super.R;
            byte green = super.G;
            byte blue = super.B;
            double black = Math.Min(1d - (red / 255d), Math.Min(1d - (green / 255d), 1d - (blue / 255d)));
            if (black == 1d)
            {
                return new(0d, 0d, 0d, 1d);
            }

            double cyan = (1d - (red / 255d) - black) / (1d - black);
            double magenta = (1d - (green / 255d) - black) / (1d - black);
            double yellow = (1d - (blue / 255d) - black) / (1d - black);
            return new(cyan, magenta, yellow, black);
        }

        /// <summary>Converts the RGB channels to hue, saturation, and value.</summary>
        /// <param name="super">The source color. Its alpha channel is ignored.</param>
        public static HSVColor ToHSVColor(this Color super)
        {
            int maximum = Math.Max(super.R, Math.Max(super.G, super.B));
            int minimum = Math.Min(super.R, Math.Min(super.G, super.B));
            double saturation = maximum == 0 ? 0d : (maximum - minimum) / (double)maximum;
            return new(super.GetHue(), saturation, maximum / 255d);
        }

        /// <summary>Adjusts the value component by the supplied amount and converts the result to an opaque color.</summary>
        /// <param name="super">The source color.</param>
        /// <param name="d">The amount to add to the value component before clamping it to the range from 0 to 1.</param>
        public static Color WithValue(this Color super, double d)
        {
            HSVColor hsv = super.ToHSVColor();
            hsv.Value += d;
            return hsv.ToColor();
        }

        /// <summary>Creates a color model from the source hue, Color.GetSaturation result, and largest RGB channel.</summary>
        /// <param name="super">The source color. Its alpha channel is ignored.</param>
        /// <param name="color"></param>
        /// <remarks>The saturation component uses the HSL saturation reported by <see cref="Color.GetSaturation"/>.</remarks>
        public static bool IsLighterThan(this Color super, Color color) => super.ToHSVColor().Value > color.ToHSVColor().Value;

        /// <summary>Checks whether the source color has a larger maximum RGB channel than the comparison color.</summary>
        /// <param name="super">The source color.</param>
        /// <param name="color">The color to compare against.</param>
        public static bool IsDarkerThan(this Color super, Color color) => color.IsLighterThan(super);

        /// <summary>Blends the RGB channels of two colors and returns an opaque color.</summary>
        /// <param name="super">The first color.</param>
        /// <param name="color">The second color.</param>
        /// <param name="d">The weight of the second color, clamped to the range from 0 to 1.</param>
        public static Color Mixed(this Color super, Color color, double d = 0.5)
        {
            d = Math.Min(Math.Max(d, 0), 1);
            double d0 = 1 - d;
            byte r = (byte)Math.Round(Math.Min((int)Math.Round((super.R * d0) + (color.R * d)), 255d));
            byte g = (byte)Math.Round(Math.Min((int)Math.Round((super.G * d0) + (color.G * d)), 255d));
            byte b = (byte)Math.Round(Math.Min((int)Math.Round((super.B * d0) + (color.B * d)), 255d));
            return Color.FromArgb(r, g, b);
        }

        /// <summary>Multiplies the corresponding RGB channels, normalizes them to the byte range, and returns an opaque color.</summary>
        /// <param name="super">The first color.</param>
        /// <param name="color">The second color.</param>
        public static Color Multiplied(this Color super, Color color)
        {
            byte r = (byte)Math.Round(Math.Min((int)Math.Round(super.R * color.R / 255d), 255d));
            byte g = (byte)Math.Round(Math.Min((int)Math.Round(super.G * color.G / 255d), 255d));
            byte b = (byte)Math.Round(Math.Min((int)Math.Round(super.B * color.B / 255d), 255d));
            return Color.FromArgb(r, g, b);
        }

        /// <summary>Converts a color to its HTML representation.</summary>
        /// <param name="super">The color to convert.</param>
        public static string ToHtml(this Color super) => ColorTranslator.ToHtml(super);

        /// <summary>Returns the eight-digit uppercase hexadecimal ARGB representation without a prefix.</summary>
        /// <param name="super">The color to convert.</param>
        public static string ToHex(this Color super) => super.ToArgb().ToString("X8");
    }
}
