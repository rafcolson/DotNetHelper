using System.Drawing.Drawing2D;

namespace WinFormsLib
{
    /// <summary>A combo box with a custom flat appearance and text history.</summary>
    public partial class FlatComboBox : ComboBox
    {

        [System.Runtime.InteropServices.LibraryImport("uxtheme.dll", StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf16)]
        private static partial int SetWindowTheme(nint hwnd, string pszSubAppName, string pszSubIdList);

        [System.Runtime.InteropServices.LibraryImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static partial bool ValidateRect(nint hwnd, nint rect);

        private const int WM_PAINT = 0xF;
        private const int WM_ERASEBKGND = 0x14;
        private const int WM_WINDOWPOSCHANGED = 0x47;
        private const int UNDO_LEVELS_MAX = 30;

        private readonly List<string> _history = [];

        private int historyIndex;
        private ComboBoxStyle _dropDownStyle;
        private Color _backColor;

        /// <summary>Gets or sets the border color used when the enabled control is hovered, focused, or open.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color ActiveBorderColor { get; set; } = SystemColors.ActiveBorder;
        /// <summary>Gets or sets the border color used when the control is inactive or disabled.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color InactiveBorderColor { get; set; } = SystemColors.InactiveBorder;
        /// <summary>Gets or sets the background color of the drop-down button.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color ButtonFaceColor { get; set; } = SystemColors.ButtonFace;
        /// <summary>Gets or sets the background color applied when the control is disabled.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color DisabledBackColor { get; set; } = SystemColors.Control;

        /// <summary>Initializes a combo box with flat styling and buffered painting.</summary>
        public FlatComboBox() : base()
        {
            _dropDownStyle = DropDownStyle;
            _backColor = BackColor;
            base.FlatStyle = FlatStyle.Flat;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Prevent the native ComboBox from animating a light hover/focus
            // border underneath the custom rendering.
            _ = SetWindowTheme(Handle, string.Empty, string.Empty);
        }

        /// <summary>Gets or sets the maximum number of history entries, clamped to the range from 0 to 30.</summary>
        /// <remarks>Reducing the limit discards the oldest entries and resets the history position to the newest retained entry.</remarks>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public int UndoLevelsCount
        {
            get;
            set
            {
                field = Math.Min(Math.Max(value, 0), UNDO_LEVELS_MAX);
                if (_history.Count > field)
                {
                    _history.RemoveRange(0, _history.Count - field);
                }
                historyIndex = Math.Max(_history.Count - 1, 0);
            }
        } = 10;

        /// <summary>Gets a copy of the saved text history in oldest-first order.</summary>
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string[] History => [.. _history];

        /// <summary>Gets whether undo can restore saved text or move to an earlier entry.</summary>
        [System.ComponentModel.Browsable(false)]
        public bool CanUndo => _history.Count != 0 && (historyIndex > 0 || Text != _history[historyIndex]);

        /// <summary>Gets whether a later history entry exists and the text matches the current entry.</summary>
        [System.ComponentModel.Browsable(false)]
        public bool CanRedo => _history.Count != 0 && historyIndex < _history.Count - 1 && Text == _history[historyIndex];

        /// <summary>Gets or sets the underlying combo box's flat style.</summary>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        /// <remarks>Repaints the border after raising the event.</remarks>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            DrawBorderNow();
        }

        /// <inheritdoc/>
        /// <remarks>Repaints the border after raising the event.</remarks>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            DrawBorderNow();
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            DrawBorderNow();
        }

        /// <inheritdoc/>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            DrawBorderNow();
        }

        /// <inheritdoc/>
        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);
            DrawBorderNow();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e)
        {
            ToggleEnabled();
            base.OnEnabledChanged(e);
        }

        /// <inheritdoc/>
        protected override void OnParentEnabledChanged(EventArgs e)
        {
            ToggleEnabled();
            base.OnParentEnabledChanged(e);
        }

        /// <summary>Saves the current nonempty text if it differs from the current history entry and history is enabled.</summary>
        public void UpdateHistory()
        {
            if (UndoLevelsCount != 0 && Text.Length != 0 && (_history.Count == 0 || Text != _history[historyIndex]))
            {
                if (_history.Count >= UndoLevelsCount)
                {
                    _history.RemoveAt(0);
                }

                _history.Add(Text);
                historyIndex = _history.Count - 1;
            }
        }

        /// <summary>Replaces the saved history with the newest nonempty entries up to the configured limit.</summary>
        /// <param name="history">The source history in oldest-first order.</param>
        public void ReplaceHistory(IEnumerable<string> history)
        {
            _history.Clear();
            _history.AddRange(history.Where(item => !string.IsNullOrEmpty(item)).TakeLast(UndoLevelsCount));
            historyIndex = Math.Max(_history.Count - 1, 0);
        }

        /// <summary>Selects the last matching history entry and uses it as the control text.</summary>
        /// <param name="item">The history text to find.</param>
        public void SelectHistoryItem(string item)
        {
            int index = _history.LastIndexOf(item);
            if (index >= 0)
            {
                historyIndex = index;
                Text = _history[index];
            }
        }

        /// <summary>Deletes the selected text.</summary>
        public void Delete()
        {
            SelectedText = string.Empty;
        }

        /// <summary>Copies the selected text to the clipboard and deletes it.</summary>
        public void Cut()
        {
            string s = SelectedText;
            if (s.Length != 0)
            {
                Clipboard.SetText(s);
                SelectedText = string.Empty;
            }
        }

        /// <summary>Copies the selected text to the clipboard.</summary>
        public void Copy()
        {
            string s = SelectedText;
            if (s.Length != 0)
            {
                Clipboard.SetText(s);
            }
        }

        /// <summary>Replaces the selected text with clipboard text, joining lines from rich text with spaces.</summary>
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

        /// <summary>Restores the current history entry if the text differs, or moves to the previous entry.</summary>
        public void Undo()
        {
            if (_history.Count != 0)
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

        /// <summary>Moves to the next history entry if one exists and uses it as the control text.</summary>
        public void Redo()
        {
            if (_history.Count != 0 & historyIndex < _history.Count - 1)
            {
                historyIndex += 1;
                Text = _history[historyIndex];
            }
        }

    }
}
