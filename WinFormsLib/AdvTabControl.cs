using System.ComponentModel;

namespace WinFormsLib
{

    /// <summary>
    /// TabControl with owner-drawn tabs and configurable colors.
    /// Provides the subset of TabControlEX behaviour used by BibleQuotes.
    /// </summary>
    public class AdvTabControl : TabControl
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern nint SendMessage(nint hwnd, int message, nint wParam, nint lParam);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(nint handle);

        private const int WM_SETFONT = 0x30;
        private Color _backColor = SystemColors.Control;
        private int _hotTabIndex = -1;
        private Point _padding;
        private readonly bool _paddingInitialized;
        private Size _itemSize;
        private readonly bool _itemSizeInitialized;
        private nint _layoutFontHandle;

        public AdvTabControl() : base()
        {
            _padding = base.Padding;
            _paddingInitialized = true;
            _itemSize = base.ItemSize;
            _itemSizeInitialized = true;
            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Normal;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Control")]
        public override Color BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                if (value.IsEmpty)
                {
                    value = SystemColors.Control;
                }

                if (_backColor != value)
                {
                    _backColor = value;
                    Invalidate(true);
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "ControlDark")]
        public Color FlatBorderColor
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    Invalidate();
                }
            }
        } = SystemColors.ControlDark;

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "ControlLight")]
        public Color HotColor
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    Invalidate();
                }
            }
        } = SystemColors.ControlLight;

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Control")]
        public Color SelectedTabColor
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    Invalidate();
                }
            }
        } = SystemColors.Control;

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Control")]
        public Color TabColor
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    Invalidate();
                }
            }
        } = SystemColors.Control;

        [Category("Appearance")]
        [DefaultValue(typeof(FontStyle), "Regular")]
        public FontStyle SelectedTabFontStyle
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    UpdateTabPadding();
                    Invalidate();
                }
            }
        } = FontStyle.Regular;

        [Category("Appearance")]
        [DefaultValue(typeof(Point), "6, 3")]
        public new Point Padding
        {
            get
            {
                return _padding;
            }
            set
            {
                if (_padding != value)
                {
                    _padding = value;
                    UpdateTabPadding();
                }
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new Size ItemSize
        {
            get
            {
                return _itemSize;
            }
            set
            {
                if (_itemSize != value)
                {
                    _itemSize = value;
                    UpdateTabPadding();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool UseVisualStyles
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    Invalidate();
                }
            }
        } = true;

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);
            DrawTab(e.Graphics, e.Index);
        }

        private void DrawTab(Graphics graphics, int index)
        {
            if (index < 0 || index >= TabPages.Count)
            {
                return;
            }

            bool selected = index == SelectedIndex;
            bool hot = index == _hotTabIndex;
            Rectangle bounds = GetTabRect(index);
            Color fillColor = selected ? SelectedTabColor : hot ? HotColor : TabColor;
            using (SolidBrush backgroundBrush = new(fillColor))
            {
                graphics.FillRectangle(backgroundBrush, bounds);
            }

            using (Pen borderPen = new(FlatBorderColor))
            {
                Rectangle border = bounds;
                border.Width -= 1;
                border.Height -= 1;
                graphics.DrawRectangle(borderPen, border);
            }

            TabPage page = TabPages[index];
            Color textColor = page.Enabled ? ForeColor : SystemColors.GrayText;
            FontStyle style = selected ? SelectedTabFontStyle : Font.Style;

            using (Font tabFont = new(Font, style))
            {
                TextRenderer.DrawText(graphics, page.Text, tabFont, bounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            }

            if (Focused && selected)
            {
                Rectangle focusBounds = bounds;
                int focusInset = Math.Max((int)Math.Round(Math.Round(3 * DeviceDpi / 96.0d)), 1);
                focusBounds.Inflate(-focusInset, -focusInset);
                ControlPaint.DrawFocusRectangle(graphics, focusBounds, textColor, fillColor);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
            {
                return;
            }

            using (SolidBrush backgroundBrush = new(_backColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, ClientRectangle);
            }

            if (!DisplayRectangle.IsEmpty)
            {
                Rectangle pageBorder = new(DisplayRectangle.X - 1, DisplayRectangle.Y - 1, DisplayRectangle.Width + 1, DisplayRectangle.Height + 1);
                using Pen borderPen = new(FlatBorderColor);
                e.Graphics.DrawRectangle(borderPen, pageBorder);
            }

            for (int index = 0, loopTo = TabCount - 1; index <= loopTo; index++)
            {
                DrawTab(e.Graphics, index);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int newHotTabIndex = GetTabIndexAt(e.Location);
            if (newHotTabIndex != _hotTabIndex)
            {
                int oldHotTabIndex = _hotTabIndex;
                _hotTabIndex = newHotTabIndex;
                InvalidateTab(oldHotTabIndex);
                InvalidateTab(_hotTabIndex);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            int oldHotTabIndex = _hotTabIndex;
            _hotTabIndex = -1;
            InvalidateTab(oldHotTabIndex);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateTabPadding();
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateTabPadding();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            ReleaseLayoutFont();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            UpdateTabPadding();
            Invalidate();
        }

        internal void UpdateTabPadding()
        {
            if (!_paddingInitialized || !_itemSizeInitialized)
            {
                return;
            }

            double dpiScale = DeviceDpi / 96.0d;
            Point scaledPadding = new((int)Math.Round(Math.Round(_padding.X * dpiScale)), (int)Math.Round(Math.Round(_padding.Y * dpiScale)));
            Size scaledItemSize = new((int)Math.Round(Math.Round(_itemSize.Width * dpiScale)), (int)Math.Round(Math.Round(_itemSize.Height * dpiScale)));

            if (base.Padding != scaledPadding)
            {
                base.Padding = scaledPadding;
            }
            if (base.ItemSize != scaledItemSize)
            {
                base.ItemSize = scaledItemSize;
            }

            UpdateLayoutFont();
        }

        public void RefreshTabLayout()
        {
            UpdateTabPadding();
            PerformLayout();
            Invalidate(true);
        }

        private void UpdateLayoutFont()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            nint newLayoutFontHandle;
            using (Font layoutFont = new(Font, SelectedTabFontStyle))
            {
                newLayoutFontHandle = layoutFont.ToHfont();
            }

            _ = SendMessage(Handle, WM_SETFONT, newLayoutFontHandle, new nint(1));

            nint oldLayoutFontHandle = _layoutFontHandle;
            _layoutFontHandle = newLayoutFontHandle;
            if (oldLayoutFontHandle != nint.Zero)
            {
                _ = DeleteObject(oldLayoutFontHandle);
            }
        }

        private void ReleaseLayoutFont()
        {
            if (_layoutFontHandle != nint.Zero)
            {
                _ = DeleteObject(_layoutFontHandle);
                _layoutFontHandle = nint.Zero;
            }
        }

        private int GetTabIndexAt(Point location)
        {
            for (int index = 0, loopTo = TabCount - 1; index <= loopTo; index++)
            {
                if (GetTabRect(index).Contains(location))
                {
                    return index;
                }
            }

            return -1;
        }

        private void InvalidateTab(int index)
        {
            if (index >= 0 && index < TabCount)
            {
                Invalidate(GetTabRect(index));
            }
        }
    }
}
