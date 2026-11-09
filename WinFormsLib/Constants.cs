using System.Globalization;

namespace WinFormsLib
{
    /// <summary>Defines shared Windows Forms defaults and utility constants.</summary>
    public static class Constants
    {
        /// <summary>The default tooltip display duration in milliseconds.</summary>
        public const int TOOLTIP_AUTO_POP_DELAY = 10000;
        /// <summary>The default delay in milliseconds before a tooltip first appears.</summary>
        public const int TOOLTIP_INITIAL_DELAY = 250;
        /// <summary>The default delay in milliseconds before another tooltip appears.</summary>
        public const int TOOLTIP_RESHOW_DELAY = 500;
        /// <summary>The default panel menu leave delay in milliseconds.</summary>
        public const int PANEL_MENU_MOUSE_LEAVE_DELAY = 1000;
        /// <summary>The empty string.</summary>
        public const string EMPTY_STRING = "";
        /// <summary>A line-feed character used as a newline separator.</summary>
        public const string NEW_LINE = "\n";
        /// <summary>The SendKeys token for the Escape key.</summary>
        public const string ESCAPE_KEY = "{ESC}";
        /// <summary>The executable name for Windows File Explorer.</summary>
        public const string EXPLORER_EXE = "explorer.exe";
        /// <summary>The characters that are invalid in a file name.</summary>
        public static readonly string INVALID_CHARACTERS = new(Path.GetInvalidFileNameChars());
        /// <summary>The directory selected by the library's default directory lookup.</summary>
        public static readonly string STARTUP_DIRECTORY_DEFAULT = Utils.GetValidDirectoryPath();
        /// <summary>The invariant culture used for culture-independent operations.</summary>
        public static readonly CultureInfo CULTURE_INFO_DEFAULT = CultureInfo.InvariantCulture;
        /// <summary>The shared default font, Segoe UI Regular at 12 points.</summary>
        /// <remarks>Callers share this instance and should not dispose it.</remarks>
        public static readonly Font FONT_DEFAULT = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        /// <summary>Uniform padding of three pixels.</summary>
        public static readonly Padding PADDING_MINIMUM = new(3);
        /// <summary>Uniform padding of six pixels.</summary>
        public static readonly Padding PADDING_DEFAULT = new(6);
        /// <summary>Uniform padding of nine pixels.</summary>
        public static readonly Padding PADDING_MAXIMUM = new(9);
        /// <summary>The minimum box size of 256 pixels wide with no minimum height.</summary>
        public static readonly Size BOX_SIZE_MINIMUM = new(256, 0);
        /// <summary>The minimum button size of 80 by 32 pixels.</summary>
        public static readonly Size BUTTON_SIZE_MINIMUM = new(80, 32);
    }
}
