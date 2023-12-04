namespace WinFormsLib
{
    public static partial class FontExtensions
    {
        public static Font ToRegular(this Font super) => new(super.FontFamily, super.Size, FontStyle.Regular);

        public static Font ToItalic(this Font super) => new(super.FontFamily, super.Size, FontStyle.Italic);

        public static Font ToBold(this Font super) => new(super.FontFamily, super.Size, FontStyle.Bold);

        public static Font ToUnderline(this Font super) => new(super.FontFamily, super.Size, FontStyle.Underline);
    }
}
