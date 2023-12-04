using System.Globalization;

namespace WinFormsLib
{
    public static class Constants
    {
        public const int TOOLTIP_AUTO_POP_DELAY = 10000;
        public const int TOOLTIP_INITIAL_DELAY = 250;
        public const int TOOLTIP_RESHOW_DELAY = 500;
        public const string EMPTY_STRING = "";
        public const string NEW_LINE = "\n";
        public const string ESCAPE_KEY = "{ESC}";
        public const string EXPLORER_EXE = "explorer.exe";
        public static readonly string INVALID_CHARACTERS = new string(new[] {
            Chars.BACKSLASH,
            Chars.SLASH,
            Chars.COLON,
            Chars.ASTERISK,
            Chars.QUESTION_MARK
        }) + Path.GetInvalidPathChars();
        public static readonly string STARTUP_DIRECTORY_DEFAULT = Utils.GetValidDirectoryPath();
        public static readonly CultureInfo CULTURE_INFO_DEFAULT = CultureInfo.InvariantCulture;
        public static readonly Font FONT_DEFAULT = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Padding PADDING_MINIMUM = new(3);
        public static readonly Padding PADDING_DEFAULT = new(6);
        public static readonly Padding PADDING_MAXIMUM = new(9);
        public static readonly Size BOX_SIZE_MINIMUM = new(256, 0);
        public static readonly Size BUTTON_SIZE_MINIMUM = new(80, 32);
    }
}
