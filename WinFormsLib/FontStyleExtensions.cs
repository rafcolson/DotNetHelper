namespace WinFormsLib
{
    public static partial class FontStyleExtensions
    {
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

        public static void FromString(this ref FontStyle super, string styles)
        {
            bool italic = new();
            bool bold = new();
            bool underline = new();
            bool strikeout = new();
            styles.Strip(Chars.SPACE);
            foreach (string style in styles.Split(Chars.COMMA))
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
