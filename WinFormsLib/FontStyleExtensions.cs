namespace WinFormsLib
{
    /// <summary>Builds font styles from individual settings or style names.</summary>
    public static partial class FontStyleExtensions
    {
        /// <summary>Replaces the target style with the selected flags, or regular style if none are selected.</summary>
        /// <param name="super">The style to update.</param>
        /// <param name="bold">Whether to set the bold flag.</param>
        /// <param name="italic">Whether to set the italic flag.</param>
        /// <param name="underline">Whether to set the underline flag.</param>
        /// <param name="strikeout">Whether to set the strikeout flag.</param>
        public static void FromProps(this ref FontStyle super, bool bold, bool italic, bool underline, bool strikeout)
        {
            super = FontStyle.Regular;
            if (bold)
            {
                super |= FontStyle.Bold;
            }
            if (italic)
            {
                super |= FontStyle.Italic;
            }
            if (underline)
            {
                super |= FontStyle.Underline;
            }
            if (strikeout)
            {
                super |= FontStyle.Strikeout;
            }
        }

        /// <summary>Replaces the target style with the flags named in a comma-separated list.</summary>
        /// <param name="super">The style to update.</param>
        /// <param name="styles">A list of case-sensitive names: <c>Bold</c>, <c>Italic</c>, <c>Underline</c>, or <c>Strikeout</c>.</param>
        /// <remarks>Surrounding whitespace and unrecognized entries are ignored.</remarks>
        public static void FromString(this ref FontStyle super, string styles)
        {
            bool italic = new();
            bool bold = new();
            bool underline = new();
            bool strikeout = new();
            foreach (string style in styles.Split(Chars.COMMA, StringSplitOptions.TrimEntries))
            {
                if (style.Equals("Bold"))
                {
                    bold = true;
                }
                else if (style.Equals("Italic"))
                {
                    italic = true;
                }
                else if (style.Equals("Underline"))
                {
                    underline = true;
                }
                else if (style.Equals("Strikeout"))
                {
                    strikeout = true;
                }
            }
            super.FromProps(bold, italic, underline, strikeout);
        }
    }
}
