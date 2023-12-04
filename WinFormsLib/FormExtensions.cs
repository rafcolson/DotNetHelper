namespace WinFormsLib
{
    public static partial class FormExtensions
    {
        public static void ShowToolTip(this Form super, string message)
        {
            using ToolTip toolTip = new();
            toolTip.Show(message, super, Cursor.Position.X - super.Location.X, Cursor.Position.Y - super.Location.Y, 1000);
        }

        public static void CenterOnScreen(this Form super)
        {
            Rectangle r = Screen.FromControl(super).WorkingArea;
            super.Location = new Point()
            {
                X = Math.Max(r.X, r.X + (r.Width - super.Width) / 2),
                Y = Math.Max(r.Y, r.Y + (r.Height - super.Height) / 2)
            };
        }

    }
}
