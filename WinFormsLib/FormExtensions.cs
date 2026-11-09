namespace WinFormsLib
{
    /// <summary>Provides positioning and tooltip helpers for Windows Forms.</summary>
    public static partial class FormExtensions
    {
        /// <summary>Requests a tooltip near the cursor with a display duration of one second.</summary>
        /// <param name="super">The form that owns the tooltip.</param>
        /// <param name="message">The text to display.</param>
        /// <remarks>Call on the form's UI thread. The tooltip is disposed after one second or when the form is disposed, whichever comes first.</remarks>
        public static void ShowToolTip(this Form super, string message)
        {
            const int duration = 1000;
            ToolTip toolTip = new();
            System.Windows.Forms.Timer timer = new() { Interval = duration };

            void DisposeToolTip(object? sender, EventArgs e)
            {
                super.Disposed -= DisposeToolTip;
                timer.Tick -= DisposeToolTip;
                timer.Dispose();
                toolTip.Dispose();
            }

            super.Disposed += DisposeToolTip;
            timer.Tick += DisposeToolTip;
            try
            {
                Point position = super.PointToClient(Cursor.Position);
                toolTip.Show(message, super, position, duration);
                timer.Start();
            }
            catch
            {
                DisposeToolTip(null, EventArgs.Empty);
                throw;
            }
        }

        /// <summary>Centers the form in the working area of its current screen.</summary>
        /// <param name="super">The form to position.</param>
        /// <remarks>If the form exceeds the working area in either dimension, it is aligned to the corresponding left or top edge.</remarks>
        public static void CenterOnScreen(this Form super)
        {
            Rectangle r = Screen.FromControl(super).WorkingArea;
            super.Location = new Point()
            {
                X = Math.Max(r.X, r.X + ((r.Width - super.Width) / 2)),
                Y = Math.Max(r.Y, r.Y + ((r.Height - super.Height) / 2))
            };
        }

    }
}
