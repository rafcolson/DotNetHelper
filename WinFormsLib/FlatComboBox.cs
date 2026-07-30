using System.Drawing.Drawing2D;

namespace WinFormsLib
{
    public class FlatComboBox : ComboBox
    {

        [System.Runtime.InteropServices.DllImport("uxtheme.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int SetWindowTheme(nint hwnd, string pszSubAppName, string pszSubIdList);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ValidateRect(nint hwnd, nint rect);

        private const int WM_PAINT = 0xF;
        private const int WM_ERASEBKGND = 0x14;
        private const int WM_WINDOWPOSCHANGED = 0x47;
        private const int UNDO_LEVELS_MAX = 30;

        private readonly List<string> _history = [];

        private int historyIndex;
        private ComboBoxStyle _dropDownStyle;
        private Color _backColor;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color ActiveBorderColor { get; set; } = SystemColors.ActiveBorder;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color InactiveBorderColor { get; set; } = SystemColors.InactiveBorder;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color ButtonFaceColor { get; set; } = SystemColors.ButtonFace;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color DisabledBackColor { get; set; } = SystemColors.Control;

        public FlatComboBox() : base()
        {
            _dropDownStyle = DropDownStyle;
            _backColor = BackColor;
            base.FlatStyle = FlatStyle.Flat;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Prevent the native ComboBox from animating a light hover/focus
            // border underneath the custom rendering.
            _ = SetWindowTheme(Handle, string.Empty, string.Empty);
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public int UndoLevelsCount
        {
            get;
            set
            {
                field = Math.Min(Math.Max(value, 1), UNDO_LEVELS_MAX);
                if (_history.Count > field)
                {
                    for (int i = 0, loopTo = _history.Count - field; i <= loopTo; i++)
                    {
                        _ = _history.Remove(0.ToString());
                    }
                }
                historyIndex = _history.Count - 1;
            }
        } = 10;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle
        {
            get
            {
                return base.FlatStyle;
            }
            set
            {
                base.FlatStyle = value;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ERASEBKGND)
            {
                // The complete control is painted from an off-screen buffer. Letting
                // Windows erase it first exposes the native background for one frame.
                m.Result = new nint(1);
                return;
            }

            if (m.Msg == WM_PAINT)
            {
                PaintBuffered();
                _ = ValidateRect(Handle, nint.Zero);
                m.Result = nint.Zero;
                return;
            }

            base.WndProc(ref m);

            if (m.Msg == WM_WINDOWPOSCHANGED)
            {
                PaintBuffered();
            }
        }

        private void PaintBuffered()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                return;
            }

            using Bitmap buffer = new(ClientSize.Width, ClientSize.Height);
            using (Graphics g = Graphics.FromImage(buffer))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                using (SolidBrush b = new(BackColor))
                {
                    g.FillRectangle(b, ClientRectangle);
                }

                DrawButton(g);
                DrawBorder(g);

                if (DropDownStyle.Equals(ComboBoxStyle.DropDownList))
                {
                    using SolidBrush b = new(ForeColor);
                    g.DrawString(Text, Font, b, ClientRectangle);
                }
            }

            using Graphics target = Graphics.FromHwnd(Handle);
            target.DrawImageUnscaled(buffer, Point.Empty);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            DrawBorderNow();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            DrawBorderNow();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            DrawBorderNow();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            DrawBorderNow();
        }

        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);
            DrawBorderNow();
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);
            DrawBorderNow();
        }

        private void DrawButton(Graphics graphics)
        {
            int buttonWidth = ScaleLogical(18);
            int arrowHalfWidth = ScaleLogical(4);
            int arrowHalfHeight = ScaleLogical(2);
            float arrowCenterX = ClientSize.Width - (buttonWidth / 2.0f);
            float arrowCenterY = ClientSize.Height / 2.0f;

            using (SolidBrush buttonBrush = new(ButtonFaceColor))
            {
                graphics.FillRectangle(buttonBrush, ClientSize.Width - buttonWidth, 0, buttonWidth, ClientSize.Height);
            }

            using GraphicsPath arrowPath = new();
            PointF topLeft = new(arrowCenterX - arrowHalfWidth, arrowCenterY - arrowHalfHeight);
            PointF topRight = new(arrowCenterX + arrowHalfWidth, arrowCenterY - arrowHalfHeight);
            PointF bottom = new(arrowCenterX, arrowCenterY + arrowHalfHeight);

            arrowPath.AddLine(topLeft, bottom);
            arrowPath.AddLine(bottom, topRight);

            using Pen arrowPen = new(Enabled ? ForeColor : SystemColors.GrayText);
            graphics.DrawPath(arrowPen, arrowPath);
        }

        private int ScaleLogical(int value)
        {
            return Math.Max((int)Math.Round(Math.Round(value * DeviceDpi / 96.0d)), 1);
        }

        private void DrawBorder(Graphics graphics)
        {
            bool mouseOver = RectangleToScreen(ClientRectangle).Contains(MousePosition);
            bool highlighted = Enabled && (mouseOver || ContainsFocus || DroppedDown);
            Color borderColor = highlighted ? ActiveBorderColor : InactiveBorderColor;

            using Pen borderPen = new(borderColor, 1f);
            graphics.DrawRectangle(borderPen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        private void DrawBorderNow()
        {
            if (IsHandleCreated && ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                using Graphics graphics = CreateGraphics();
                DrawBorder(graphics);
            }
        }

        private void ToggleEnabled()
        {
            if (Enabled)
            {
                DropDownStyle = _dropDownStyle;
                BackColor = _backColor;
            }
            else
            {
                _dropDownStyle = DropDownStyle;
                _backColor = BackColor;
                DropDownStyle = ComboBoxStyle.DropDownList;
                BackColor = DisabledBackColor;
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            ToggleEnabled();
            base.OnEnabledChanged(e);
        }

        protected override void OnParentEnabledChanged(EventArgs e)
        {
            ToggleEnabled();
            base.OnParentEnabledChanged(e);
        }

        public void UpdateHistory()
        {
            if (UndoLevelsCount != 0 & Text.Any() & !(_history.Any() && (Text ?? "") == (_history[historyIndex] ?? "")))
            {
                if (_history.Count == UndoLevelsCount)
                {
                    _history.RemoveAt(0);
                }

                _history.Add(Text);
                historyIndex = _history.Count - 1;
            }
        }

        public void Delete()
        {
            SelectedText = string.Empty;
        }

        public void Cut()
        {
            string s = SelectedText;
            if (s.Any())
            {
                Clipboard.SetText(s);
                SelectedText = string.Empty;
            }
        }

        public void Copy()
        {
            string s = SelectedText;
            if (s.Any())
            {
                Clipboard.SetText(s);
            }
        }

        public void Paste()
        {
            if (Clipboard.ContainsText(TextDataFormat.Rtf))
            {
                RichTextBox rtb = new() { Rtf = Clipboard.GetText(TextDataFormat.Rtf) };
                SelectedText = string.Join(' ', rtb.Lines);
                rtb.Dispose();
            }
            else
            {
                SelectedText = Clipboard.GetText();
            }
        }

        public void Undo()
        {
            if (_history.Any())
            {
                string s = _history[historyIndex];
                if ((s ?? "") != (Text ?? ""))
                {
                    Text = s;
                }
                else if (historyIndex > 0)
                {
                    historyIndex -= 1;
                    Text = _history[historyIndex];
                }
            }
        }

        public void Redo()
        {
            if (_history.Any() & historyIndex < _history.Count - 1)
            {
                historyIndex += 1;
                Text = _history[historyIndex];
            }
        }

    }
}
