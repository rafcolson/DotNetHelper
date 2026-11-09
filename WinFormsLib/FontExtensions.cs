namespace WinFormsLib
{
    /// <summary>Creates fonts with a specified style.</summary>
    public static partial class FontExtensions
    {
        /// <summary>Creates a font with the same family and numeric size using only the regular style.</summary>
        /// <param name="super">The source font.</param>
        /// <returns>A new font that the caller must dispose.</returns>
        public static Font ToRegular(this Font super) => new(super.FontFamily, super.Size, FontStyle.Regular);

        /// <summary>Creates a font with the same family and numeric size using only the italic style.</summary>
        /// <param name="super">The source font.</param>
        /// <returns>A new font that the caller must dispose.</returns>
        public static Font ToItalic(this Font super) => new(super.FontFamily, super.Size, FontStyle.Italic);

        /// <summary>Creates a font with the same family and numeric size using only the bold style.</summary>
        /// <param name="super">The source font.</param>
        /// <returns>A new font that the caller must dispose.</returns>
        public static Font ToBold(this Font super) => new(super.FontFamily, super.Size, FontStyle.Bold);

        /// <summary>Creates a font with the same family and numeric size using only the underline style.</summary>
        /// <param name="super">The source font.</param>
        /// <returns>A new font that the caller must dispose.</returns>
        public static Font ToUnderline(this Font super) => new(super.FontFamily, super.Size, FontStyle.Underline);
    }
}
