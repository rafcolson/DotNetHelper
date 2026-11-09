namespace WinFormsLib
{
    /// <summary>Represents a color using cyan, magenta, yellow, and black components.</summary>
    /// <param name="cyan">The cyan component, from 0 to 1.</param>
    /// <param name="magenta">The magenta component, from 0 to 1.</param>
    /// <param name="yellow">The yellow component, from 0 to 1.</param>
    /// <param name="black">The black component, from 0 to 1.</param>
    /// <remarks>Component values are stored as supplied.</remarks>
    public class CMYKColor(double cyan, double magenta, double yellow, double black)
    {
        /// <summary>Gets or sets the cyan component.</summary>
        public double C { get; set; } = cyan;
        /// <summary>Gets or sets the magenta component.</summary>
        public double M { get; set; } = magenta;
        /// <summary>Gets or sets the yellow component.</summary>
        public double Y { get; set; } = yellow;
        /// <summary>Gets or sets the black component.</summary>
        public double K { get; set; } = black;

        /// <summary>Converts the components to an opaque RGB color.</summary>
        public Color ToColor()
        {
            byte r = Convert.ToByte(Math.Min((1d - Math.Min(1d, (C * (1d - K)) + K)) * 255d, 255));
            byte g = Convert.ToByte(Math.Min((1d - Math.Min(1d, (M * (1d - K)) + K)) * 255d, 255));
            byte b = Convert.ToByte(Math.Min((1d - Math.Min(1d, (Y * (1d - K)) + K)) * 255d, 255));
            return Color.FromArgb(r, g, b);
        }
    }
}
