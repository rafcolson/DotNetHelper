using System.Drawing.Drawing2D;

namespace WinFormsLib
{
    /// <summary>
    /// Uses the themed professional renderer while drawing a simple,
    /// DPI-aware selection mark for checked menu items.
    /// </summary>
    public sealed class UIToolStripRenderer(UIColorTable colorTable) : ToolStripProfessionalRenderer(colorTable)
    {
        private readonly UIColorTable _colorTable = colorTable ?? throw new ArgumentNullException(nameof(colorTable));

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Item is not ToolStripMenuItem menuItem || menuItem.CheckState == CheckState.Unchecked)
            {
                return;
            }

            Rectangle bounds = e.ImageRectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            float dpiScale = (e.Item.Owner?.DeviceDpi ?? 96) / 96F;
            float lineWidth = Math.Max(1.5F, 1.75F * dpiScale);
            Color color = e.Item.Enabled
                ? _colorTable.ColorScheme.ForeColor2
                : _colorTable.ColorScheme.ForeColor4;

            SmoothingMode previousSmoothingMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using Pen pen = new(color, lineWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            if (menuItem.CheckState == CheckState.Indeterminate)
            {
                float y = bounds.Top + (bounds.Height * 0.5F);
                e.Graphics.DrawLine(
                    pen,
                    bounds.Left + (bounds.Width * 0.24F),
                    y,
                    bounds.Right - (bounds.Width * 0.2F),
                    y);
            }
            else
            {
                PointF[] points =
                [
                    new(bounds.Left + (bounds.Width * 0.18F), bounds.Top + (bounds.Height * 0.52F)),
                    new(bounds.Left + (bounds.Width * 0.4F), bounds.Top + (bounds.Height * 0.74F)),
                    new(bounds.Left + (bounds.Width * 0.82F), bounds.Top + (bounds.Height * 0.26F))
                ];
                e.Graphics.DrawLines(pen, points);
            }

            e.Graphics.SmoothingMode = previousSmoothingMode;
        }
    }
}
