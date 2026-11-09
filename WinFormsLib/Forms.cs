using System.ComponentModel;
using static WinFormsLib.Buttons;
using static WinFormsLib.Constants;
using static WinFormsLib.ToolStripMenuItems;
using static WinFormsLib.Utils;

namespace WinFormsLib
{
    /// <summary>Provides reusable Windows Forms dialogs and editing menus.</summary>
    public static class Forms
    {
        /// <summary>Displays editing commands for a text box.</summary>
        public class EditTextContextMenuStrip : ContextMenuStrip
        {
            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                ToolStripMenuItem[] items = [.. Items.Cast<ToolStripMenuItem>()];
                Items.Clear();
                foreach (ToolStripMenuItem tsmi in items)
                {
                    tsmi.Dispose();
                }
                Opening -= EditTextContexMenuStrip_Opening;
                base.Dispose(disposing);
            }

            private void EditTextContexMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
            {
                if (sender is ContextMenuStrip cms && cms.SourceControl is TextBox tb)
                {
                    foreach (ToolStripMenuItem tsmi in cms.Items)
                    {
                        switch (tsmi.Tag)
                        {
                            case EditMenuItem.Undo:
                                tsmi.Enabled = tb.Focused && tb.CanUndo;
                                break;
                            case EditMenuItem.Cut:
                                tsmi.Enabled = tb.Focused && tb.SelectionLength != 0;
                                break;
                            case EditMenuItem.Copy:
                                tsmi.Enabled = tb.Focused && tb.SelectionLength != 0;
                                break;
                            case EditMenuItem.Paste:
                                tsmi.Enabled = tb.Focused && Clipboard.ContainsText();
                                break;
                            case EditMenuItem.SelectAll:
                                tsmi.Enabled = tb.Focused && tb.Text.Length != 0;
                                break;
                        }
                    }
                }
            }

            /// <summary>Initializes the editing menu with the supplied settings.</summary>
            /// <param name="font">The font to use, or null to use the library default.</param>
            /// <param name="editMenuItems">The editing commands to include.</param>
            public EditTextContextMenuStrip
            (
                Font? font = null,
                EditMenuItems editMenuItems = EditMenuItems.UndoCutCopyPasteSelectAll
            )
            {
                font ??= FONT_DEFAULT;
                SuspendLayout();
                Name = "EditTextContexMenuStrip";
                AutoSize = true;
                Margin = PADDING_DEFAULT;
                Font = font;
                ToolStripMenuItem[] items = GetToolStripMenuItems(editMenuItems);
                Items.AddRange(items);
                Opening += EditTextContexMenuStrip_Opening;
                ResumeLayout();
                PerformLayout();
            }
        }

        /// <summary>Displays a message with optional links and configurable dialog buttons.</summary>
        public class MessageDialog : Form
        {
            /// <summary>The link label that displays the caption.</summary>
            protected readonly LinkLabel CaptionLabel = new();
            /// <summary>The scrollable container for the caption.</summary>
            protected readonly Panel CaptionPanel = new();
            /// <summary>The container for the dialog result buttons.</summary>
            protected readonly FlowLayoutPanel DialogFlowLayoutPanel = new();
            /// <summary>The dialog's main layout container.</summary>
            protected readonly TableLayoutPanel MainTableLayoutPanel = new();

            /// <inheritdoc/>
            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                UpdateDialogLayout();
                this.CenterOnScreen();
            }

            /// <inheritdoc/>
            protected override void OnDpiChanged(DpiChangedEventArgs e)
            {
                base.OnDpiChanged(e);
                UpdateDialogLayout();
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                this.Clear();
                base.Dispose(disposing);
            }

            private void UpdateDialogLayout()
            {
                UpdateDpiLayout();
                UpdateTitleMinimumWidth();
                PerformLayout();
                UpdateCaptionMinimumSize();
                PerformLayout();
            }

            private void UpdateTitleMinimumWidth()
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return;
                }

                int titleWidth = TextRenderer.MeasureText(
                    Text,
                    SystemFonts.CaptionFont,
                    Size.Empty,
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
                int spacing = (int)Math.Ceiling(24 * DeviceDpi / 96d);
                int nonClientWidth = SystemInformation.CaptionButtonSize.Width
                    + (SystemInformation.FrameBorderSize.Width * 2)
                    + spacing;
                MinimumSize = new(
                    titleWidth + nonClientWidth,
                    MinimumSize.Height);
            }

            private int ScaleLogical(int value) => (int)Math.Round(value * DeviceDpi / 96d);

            private Padding ScaleLogical(Padding value) => new(
                ScaleLogical(value.Left),
                ScaleLogical(value.Top),
                ScaleLogical(value.Right),
                ScaleLogical(value.Bottom));

            private Size ScaleLogical(Size value) => new(ScaleLogical(value.Width), ScaleLogical(value.Height));

            private void UpdateDpiLayout()
            {
                Button[] buttons = [.. DialogFlowLayoutPanel.Controls.OfType<Button>()];
                if (buttons.Length > 0)
                {
                    Padding buttonMargin = ScaleLogical(PADDING_MINIMUM);
                    Size buttonMinimumSize = ScaleLogical(BUTTON_SIZE_MINIMUM);
                    Size buttonSize = GetMaxSize([.. buttons.Select(button => button.Text)], Font, buttonMargin, buttonMinimumSize);
                    foreach (Button button in buttons)
                    {
                        button.Margin = buttonMargin;
                        button.MinimumSize = buttonMinimumSize;
                        button.Size = buttonSize;
                    }
                }
            }

            private void UpdateCaptionMinimumSize()
            {
                if (CaptionLabel.IsDisposed || string.IsNullOrEmpty(CaptionLabel.Text))
                {
                    return;
                }

                Rectangle workingArea = Screen.FromControl(this).WorkingArea;
                Size logicalDimensions = ScaleLogical(new Size(16, 9));
                Padding scaledPadding = ScaleLogical(PADDING_DEFAULT);
                Padding additionalPadding = new(
                    scaledPadding.Left - PADDING_DEFAULT.Left,
                    scaledPadding.Top - PADDING_DEFAULT.Top,
                    scaledPadding.Right - PADDING_DEFAULT.Right,
                    scaledPadding.Bottom - PADDING_DEFAULT.Bottom);

                int calculatedCaptionWidth = CalculateSize(CaptionLabel.Text, CaptionLabel.Font, logicalDimensions, additionalPadding).Width;
                int nonClientWidth = Width - ClientSize.Width;
                int horizontalSpacing = MainTableLayoutPanel.Padding.Horizontal + CaptionPanel.Margin.Horizontal;
                int windowCaptionWidth = Math.Max(0, MinimumSize.Width - nonClientWidth - horizontalSpacing);
                int maximumCaptionWidth = Math.Max(CaptionLabel.Font.Height, workingArea.Width - nonClientWidth - horizontalSpacing);
                int captionWidth = Math.Min(Math.Max(calculatedCaptionWidth, windowCaptionWidth), maximumCaptionWidth);

                CaptionLabel.MinimumSize = Size.Empty;
                CaptionLabel.MaximumSize = new(captionWidth, 0);
                int preferredCaptionHeight = CaptionLabel.GetPreferredSize(new(captionWidth, 0)).Height;

                int nonClientHeight = Height - ClientSize.Height;
                int reservedHeight = nonClientHeight
                    + MainTableLayoutPanel.Padding.Vertical
                    + CaptionPanel.Margin.Vertical
                    + DialogFlowLayoutPanel.GetPreferredSize(Size.Empty).Height
                    + DialogFlowLayoutPanel.Margin.Vertical;
                int maximumCaptionHeight = Math.Max(CaptionLabel.Font.Height, workingArea.Height - reservedHeight);
                bool verticalScrollBarVisible = preferredCaptionHeight > maximumCaptionHeight;
                int captionPanelWidth = captionWidth + (verticalScrollBarVisible ? SystemInformation.VerticalScrollBarWidth : 0);
                CaptionPanel.MinimumSize = new(captionPanelWidth, Math.Min(preferredCaptionHeight, maximumCaptionHeight));
                CaptionPanel.MaximumSize = new(0, maximumCaptionHeight);
            }

            private void InitializeComponent()
            {
                SuspendLayout();
                CaptionLabel.SuspendLayout();
                CaptionPanel.SuspendLayout();
                DialogFlowLayoutPanel.SuspendLayout();
                MainTableLayoutPanel.SuspendLayout();

                CaptionLabel.Name = "CaptionLabel";
                CaptionLabel.AutoSize = true;
                CaptionLabel.Margin = Padding.Empty;
                CaptionLabel.LinkBehavior = LinkBehavior.HoverUnderline;
                CaptionLabel.TabStop = false;
                CaptionLabel.UseMnemonic = false;

                CaptionPanel.Name = "CaptionPanel";
                CaptionPanel.AutoScroll = true;
                CaptionPanel.AutoSize = true;
                CaptionPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                CaptionPanel.Dock = DockStyle.Fill;
                CaptionPanel.Margin = PADDING_MINIMUM;
                CaptionPanel.TabStop = false;

                DialogFlowLayoutPanel.Name = "DialogFlowLayoutPanel";
                DialogFlowLayoutPanel.AutoSize = true;
                DialogFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                DialogFlowLayoutPanel.Dock = DockStyle.Fill;
                DialogFlowLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
                DialogFlowLayoutPanel.WrapContents = false;
                DialogFlowLayoutPanel.Margin = PADDING_MINIMUM;
                DialogFlowLayoutPanel.TabStop = false;

                MainTableLayoutPanel.Name = "MainTableLayoutPanel";
                MainTableLayoutPanel.AutoSize = true;
                MainTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                MainTableLayoutPanel.Dock = DockStyle.Fill;
                MainTableLayoutPanel.Padding = PADDING_DEFAULT;
                MainTableLayoutPanel.ColumnCount = 1;
                _ = MainTableLayoutPanel.ColumnStyles.Add(new(SizeType.AutoSize));
                MainTableLayoutPanel.RowCount = 3;
                for (int i = 0; i < MainTableLayoutPanel.RowCount; i++)
                {
                    _ = MainTableLayoutPanel.RowStyles.Add(new(SizeType.AutoSize));
                }
                MainTableLayoutPanel.TabStop = false;

                Name = "MainDialog";
                AutoSize = true;
                AutoSizeMode = AutoSizeMode.GrowAndShrink;
                AutoScaleDimensions = new SizeF(96F, 96F);
                AutoScaleMode = AutoScaleMode.Dpi;
                SizeGripStyle = SizeGripStyle.Hide;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowIcon = false;
                ShowInTaskbar = false;
                TopMost = true;

                Controls.Add(MainTableLayoutPanel);
                CaptionPanel.Controls.Add(CaptionLabel);
                MainTableLayoutPanel.Controls.Add(CaptionPanel, 0, 0);
                MainTableLayoutPanel.Controls.Add(DialogFlowLayoutPanel, 0, 2);

                CaptionLabel.ResumeLayout();
                CaptionPanel.ResumeLayout();
                DialogFlowLayoutPanel.ResumeLayout();
                MainTableLayoutPanel.ResumeLayout();
                MainTableLayoutPanel.PerformLayout();
                ResumeLayout();
                PerformLayout();
            }

            /// <summary>Initializes the dialog with the supplied settings.</summary>
            /// <param name="caption">The text displayed in the dialog.</param>
            /// <param name="title">The dialog title.</param>
            /// <param name="font">The font to use, or null to use the library default.</param>
            /// <param name="buttons">The group of dialog result buttons.</param>
            public MessageDialog
            (
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null,
                DialogResultFlags buttons = DialogResultFlags.OK
            )
            {
                InitializeComponent();
                font ??= FONT_DEFAULT;
                Font = font;
                Text = title;

                CaptionLabel.Text = caption;
                CaptionLabel.UpdateLinks(linkClickedAction: () => DialogResult = DialogResult.OK);
                _ = DialogResultButtons.AddButtons(DialogFlowLayoutPanel, buttons);
            }
        }

        /// <summary>Displays text in a scrollable text box.</summary>
        public class TextDialog : MessageDialog
        {
            /// <summary>The text box used to display the caption.</summary>
            protected readonly RichTextBox CaptionTextBox;

            /// <inheritdoc/>
            protected override void OnShown(EventArgs e)
            {
                Rectangle workingArea = Screen.FromControl(this).WorkingArea;
                int scale(int value) => (int)Math.Round(value * DeviceDpi / 96d);
                string[] lines = CaptionTextBox.Lines.Length == 0 ? [string.Empty] : CaptionTextBox.Lines;
                int textWidth = lines.Max(line => TextRenderer.MeasureText(
                    line,
                    CaptionTextBox.Font,
                    Size.Empty,
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width);
                int textHeight = Math.Max(lines.Length, 4) * CaptionTextBox.Font.Height;
                int horizontalSpacing = scale(48) + SystemInformation.VerticalScrollBarWidth;
                int verticalSpacing = scale(120);

                int maximumWidth = Math.Max(MinimumSize.Width, (int)(workingArea.Width * 0.8));
                int maximumHeight = Math.Max(MinimumSize.Height, (int)(workingArea.Height * 0.7));
                Width = Math.Clamp(textWidth + horizontalSpacing, MinimumSize.Width, maximumWidth);
                Height = Math.Clamp(textHeight + verticalSpacing, MinimumSize.Height, maximumHeight);
                base.OnShown(e);
            }

            /// <summary>Initializes the dialog with the supplied settings.</summary>
            /// <param name="caption">The text displayed in the dialog.</param>
            /// <param name="title">The dialog title.</param>
            /// <param name="font">The font to use, or null to use the library default.</param>
            /// <param name="buttons">The group of dialog result buttons.</param>
            public TextDialog
            (
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null,
                DialogResultFlags buttons = DialogResultFlags.OK
            )
            : base(string.Empty, title, font, buttons)
            {
                CaptionPanel.Controls.Remove(CaptionLabel);
                CaptionLabel.Dispose();

                CaptionPanel.AutoScroll = false;
                CaptionPanel.AutoSize = false;

                AutoSize = false;
                FormBorderStyle = FormBorderStyle.Sizable;
                MaximizeBox = true;
                MinimumSize = new(480, 240);
                SizeGripStyle = SizeGripStyle.Show;
                MainTableLayoutPanel.ColumnStyles[0].SizeType = SizeType.Percent;
                MainTableLayoutPanel.ColumnStyles[0].Width = 100F;
                MainTableLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                MainTableLayoutPanel.RowStyles[0].Height = 100F;

                CaptionTextBox = new()
                {
                    Name = "CaptionTextBox",
                    BackColor = SystemColors.Control,
                    BorderStyle = BorderStyle.None,
                    DetectUrls = false,
                    Dock = DockStyle.Fill,
                    Font = Font,
                    ReadOnly = true,
                    ScrollBars = RichTextBoxScrollBars.ForcedVertical,
                    TabStop = false,
                    Text = caption
                };
                CaptionPanel.Controls.Add(CaptionTextBox);
            }
        }

        /// <summary>Displays a message and a checkbox.</summary>
        public class MessageCheckDialog : MessageDialog
        {
            /// <summary>The checkbox displayed by the dialog.</summary>
            protected readonly CheckBox CheckBox;

            /// <summary>Gets or sets the checkbox state.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool Checked
            {
                get => CheckBox.Checked;
                set => CheckBox.Checked = value;
            }

            /// <summary>Initializes the dialog with the supplied settings.</summary>
            /// <param name="checkText">The label displayed next to the checkbox.</param>
            /// <param name="caption">The text displayed in the dialog.</param>
            /// <param name="title">The dialog title.</param>
            /// <param name="font">The font to use, or null to use the library default.</param>
            /// <param name="buttons">The group of dialog result buttons.</param>
            public MessageCheckDialog
            (
                string checkText,
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null,
                DialogResultFlags buttons = DialogResultFlags.YesNo
            )
            : base(caption, title, font, buttons)
            {
                CheckBox = new()
                {
                    Name = "CheckBox",
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Margin = PADDING_DEFAULT,
                    Text = checkText,
                    TabStop = false
                };
                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionPanel);
                    CaptionPanel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(CheckBox, 0, 1);
            }
        }

        /// <summary>Displays an image with an optional caption.</summary>
        public class ImageDialog : MessageDialog
        {
            /// <summary>The picture box that displays the image.</summary>
            protected readonly PictureBox PictureBox;

            /// <summary>Initializes the dialog with the supplied settings.</summary>
            /// <param name="image">The image to display.</param>
            /// <param name="caption">The text displayed in the dialog.</param>
            /// <param name="title">The dialog title.</param>
            /// <param name="font">The font to use, or null to use the library default.</param>
            /// <param name="buttons">The group of dialog result buttons.</param>
            public ImageDialog
            (
                Image image,
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null,
                DialogResultFlags buttons = DialogResultFlags.OK
            )
            : base(caption, title, font, buttons)
            {
                PictureBox = new()
                {
                    Name = "PictureBox",
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Margin = PADDING_MAXIMUM,
                    Image = image,
                    TabStop = false
                };
                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionPanel);
                    CaptionPanel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(PictureBox, 0, 1);
            }
        }

        /// <summary>Displays a grid of selectable images.</summary>
        public class ImageTableDialog : MessageDialog
        {
            /// <summary>The container for the selectable images.</summary>
            protected readonly TableLayoutPanel ImageTableLayoutPanel;

            private readonly List<Point> _imagePositions = [];

            /// <summary>Gets the zero-based index of the selected image, or -1 when nothing is selected.</summary>
            public int SelectedIndex { get; private set; }
            /// <summary>Gets or sets the image grid's caption.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string CaptionText { get => CaptionLabel.Text; set => CaptionLabel.Text = value; }

            /// <summary>Gets or sets an optional callback invoked after an image is selected.</summary>
            /// <remarks>The returned task is not awaited by the dialog.</remarks>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Func<Task>? OnSelectedIndexChanged { get; set; } = null;

            private void PictureBox_Click(object? sender, EventArgs? e = null)
            {
                if (sender is PictureBox pbSelected)
                {
                    ImageTableLayoutPanel.SuspendDrawing();
                    SelectedIndex = ImageTableLayoutPanel.Controls.IndexOf(pbSelected);
                    for (int i = 0; i < _imagePositions.Count; i++)
                    {
                        Point p = _imagePositions[i];
                        if (ImageTableLayoutPanel.GetControlFromPosition(p.X, p.Y) is Control ctrl)
                        {
                            PictureBox pb = (PictureBox)ctrl;
                            pb.BackColor = SelectedIndex == i ? SystemColors.ControlLightLight : SystemColors.Control;
                            pb.BorderStyle = SelectedIndex == i ? BorderStyle.Fixed3D : BorderStyle.None;
                        }
                    }
                    ImageTableLayoutPanel.ResumeDrawing();
                    _ = (OnSelectedIndexChanged?.Invoke());
                }
            }

            /// <summary>Initializes the dialog with the supplied settings.</summary>
            /// <param name="images">The images to display in the grid.</param>
            /// <param name="preferredSize">The preferred size used to calculate the image cell dimensions.</param>
            /// <param name="selectedIndex">The initial zero-based selection index, or -1 for no selection.</param>
            /// <param name="caption">The text displayed in the dialog.</param>
            /// <param name="title">The dialog title.</param>
            /// <param name="font">The font to use, or null to use the library default.</param>
            /// <param name="buttons">The group of dialog result buttons.</param>
            public ImageTableDialog
            (
                Image[] images,
                Size preferredSize,
                int selectedIndex = -1,
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null,
                DialogResultFlags buttons = DialogResultFlags.OK
            )
            : base(caption, title, font, buttons)
            {
                int margin = 3;
                int numImages = images.Length;
                Size tl = GetTableLayout(numImages);
                int cc = tl.Width;
                int rc = tl.Height;
                for (int j = 0; j < rc; j++)
                {
                    for (int i = 0; i < cc; i++)
                    {
                        _imagePositions.Add(new(i, j));
                    }
                }
                int n = _imagePositions.Count - numImages;
                _imagePositions.RemoveRange(numImages, n);

                ImageTableLayoutPanel = new()
                {
                    Name = "ImageTableLayoutPanel",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Fill,
                    ColumnCount = cc,
                    RowCount = rc,
                    Margin = new(margin),
                    TabStop = false
                };
                int w = (preferredSize.Width - 24 - (margin * 6)) / cc;
                int h = (preferredSize.Height - 192 - (margin * 6)) / rc;
                for (int ic = 0; ic < cc; ic++)
                {
                    _ = ImageTableLayoutPanel.ColumnStyles.Add(new(SizeType.Absolute, w));
                }
                for (int ir = 0; ir < rc; ir++)
                {
                    _ = ImageTableLayoutPanel.RowStyles.Add(new(SizeType.Absolute, h));
                }
                for (int i = 0; i < numImages; i++)
                {
                    PictureBox pb = new()
                    {
                        Name = $"PictureBox{i}",
                        AutoSize = true,
                        Dock = DockStyle.Fill,
                        Margin = new(margin * 2),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = images[i],
                        TabStop = false
                    };
                    pb.Click += PictureBox_Click; ;
                    ImageTableLayoutPanel.Controls.Add(pb);
                }
                MainTableLayoutPanel.Controls.Add(ImageTableLayoutPanel, 0, 1);
                SelectedIndex = selectedIndex;
                if (selectedIndex >= 0)
                {
                    Point p = _imagePositions[selectedIndex];
                    PictureBox_Click(ImageTableLayoutPanel.GetControlFromPosition(p.X, p.Y));
                }
            }
        }

        /// <summary>Displays a drop-down list for choosing an item.</summary>
        public class DropDownListDialog : MessageDialog
        {
            /// <summary>The combo box that displays the choices.</summary>
            protected readonly ComboBox DropDownList;

            /// <summary>Gets or sets the zero-based index of the selected item, or -1 when nothing is selected.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int SelectedIndex { get => DropDownList.SelectedIndex; set => DropDownList.SelectedIndex = value; }
            /// <summary>Gets or sets the selected item.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public object? SelectedItem { get => DropDownList.SelectedItem; set => DropDownList.SelectedItem = value; }

            /// <summary>Initializes the dialog with the supplied settings.</summary>
            /// <param name="items">The items displayed by the dialog.</param>
            /// <param name="selectedIndex">The initial zero-based selection index, or -1 for no selection.</param>
            /// <param name="caption">The text displayed in the dialog.</param>
            /// <param name="title">The dialog title.</param>
            /// <param name="font">The font to use, or null to use the library default.</param>
            public DropDownListDialog
            (
                object[] items,
                int selectedIndex = 0,
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null
            )
            : base(caption, title, font, DialogResultFlags.OKCancel)
            {
                DropDownList = new()
                {
                    Name = "ItemsComboBox",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    MinimumSize = new(GetMaxWidth(items, Font), 1),
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Margin = PADDING_MAXIMUM,
                    TabStop = false,
                };
                DropDownList.Items.AddRange(items);
                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionPanel);
                    CaptionPanel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(DropDownList, 0, 1);
                SelectedIndex = selectedIndex;
            }
        }

        /// <summary>Edits named text values and writes them back to the supplied map on confirmation.</summary>
        public class InputDialog : MessageDialog
        {
            /// <summary>The container for the input labels and text boxes.</summary>
            protected readonly TableLayoutPanel InputTableLayoutPanel;

            /// <summary>The map that receives the entered text on confirmation.</summary>
            protected readonly Map<string, object?> _input;

            /// <inheritdoc/>
            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                if (AcceptButton != null && DialogResult == AcceptButton.DialogResult)
                {
                    for (int i = 0; i < InputTableLayoutPanel.RowCount; i++)
                    {
                        Control? keyControl = InputTableLayoutPanel.GetControlFromPosition(0, i);
                        Control? valueControl = InputTableLayoutPanel.GetControlFromPosition(1, i);
                        if (keyControl is not null && valueControl is not null)
                        {
                            string k = keyControl.Text;
                            string v = valueControl.Text;
                            _input[k] = v;
                        }
                    }
                    _ = (OnUpdate?.Invoke());
                }
                base.OnFormClosed(e);
            }

            private void TextBox_MouseDown(object? sender, MouseEventArgs e)
            {
                if (sender is TextBox tb && e.Button == MouseButtons.Right)
                {
                    _ = tb.Focus();
                }
            }

            /// <summary>Gets or sets an optional callback invoked after the entered values are written back.</summary>
            /// <remarks>The returned task is not awaited by the dialog.</remarks>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Func<Task>? OnUpdate { get; set; } = null;

            /// <summary>Initializes the dialog with the supplied settings.</summary>
            /// <param name="input">The map whose entries are displayed and updated on confirmation.</param>
            /// <param name="caption">The text displayed in the dialog.</param>
            /// <param name="title">The dialog title.</param>
            /// <param name="font">The font to use, or null to use the library default.</param>
            public InputDialog
            (
                ref Map<string, object?> input,
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null
            )
            : base(caption, title, font, DialogResultFlags.OKCancel)
            {
                _input = input;

                InputTableLayoutPanel = new()
                {
                    Name = "InputTableLayoutPanel",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = _input.Count,
                    Margin = PADDING_MINIMUM,
                    TabStop = false
                };
                KeyValuePair<string, object?>[] kvpa = [.. _input];
                ColumnStyle cs = kvpa.Length == 1 && string.IsNullOrEmpty(kvpa[0].Key) ? new(SizeType.Absolute, 1) : new(SizeType.AutoSize);
                _ = InputTableLayoutPanel.ColumnStyles.Add(cs);
                _ = InputTableLayoutPanel.ColumnStyles.Add(new(SizeType.Percent, 100F));
                for (int i = 0; i < InputTableLayoutPanel.RowCount; i++)
                {
                    _ = InputTableLayoutPanel.RowStyles.Add(new(SizeType.AutoSize));
                }

                for (int i = 0; i < kvpa.Length; i++)
                {
                    KeyValuePair<string, object?> kvp = kvpa[i];
                    string k = kvp.Key;
                    object? v = kvp.Value;
                    Label l = new()
                    {
                        Name = $"InputDialogLabel{k}",
                        AutoSize = true,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Dock = DockStyle.Fill,
                        Margin = PADDING_DEFAULT,
                        Text = k,
                        TabStop = false
                    };
                    TextBox tb = new()
                    {
                        Name = $"InputDialogTextBox{k}",
                        AutoSize = true,
                        Dock = DockStyle.Fill,
                        Margin = PADDING_DEFAULT,
                        Text = $"{v}",
                        MinimumSize = BOX_SIZE_MINIMUM,
                        TabIndex = i,
                        TabStop = true,
                        ContextMenuStrip = new EditTextContextMenuStrip(font)
                    };
                    tb.MouseDown += TextBox_MouseDown;
                    tb.Disposed += (sender, e) => tb.MouseDown -= TextBox_MouseDown;
                    InputTableLayoutPanel.Controls.Add(l, 0, i);
                    InputTableLayoutPanel.Controls.Add(tb, 1, i);
                }

                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionPanel);
                    CaptionPanel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(InputTableLayoutPanel, 0, 1);
                if (InputTableLayoutPanel.GetControlFromPosition(1, 0) is TextBox tb1)
                {
                    ActiveControl = tb1;
                }
            }
        }

        /// <summary>Edits a list and writes the edited items back on confirmation.</summary>
        public class EditListDialog : MessageDialog
        {
            /// <summary>The container for the list and its editing buttons.</summary>
            protected readonly TableLayoutPanel EditTableLayoutPanel;
            /// <summary>The container for the editing buttons.</summary>
            protected readonly FlowLayoutPanel EditFlowLayoutPanel;
            /// <summary>The list box that displays the items being edited.</summary>
            protected readonly ListBox EditListBox;

            private readonly List<object> _items;

            /// <inheritdoc/>
            protected override void OnShown(EventArgs e)
            {
                Items.Clear();
                Items.AddRange([.. _items]);
                if (Items.Count == 0)
                {
                    UpdateEditButtons();
                }
                else
                {
                    SelectedIndex = 0;
                }
                base.OnShown(e);
            }

            /// <inheritdoc/>
            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                if (AcceptButton != null && DialogResult == AcceptButton.DialogResult)
                {
                    _items.Replace(Items.Cast<object>());
                }
                base.OnFormClosed(e);
            }

            private async void AddItemAsync(object? sender, EventArgs e)
            {
                if (OnAddItem != null)
                {
                    await OnAddItem.Invoke();
                    return;
                }
                Map<string, object?> m = new() { { string.Empty, string.Empty } };
                using InputDialog inputDialog = new(ref m, font: Font);
                DialogResult dialogResult = DialogResult.None;
                while (dialogResult == DialogResult.None)
                {
                    dialogResult = inputDialog.ShowDialog();
                    if (dialogResult == DialogResult.Cancel)
                    {
                        break;
                    }
                    else if (m[string.Empty] is string s && !string.IsNullOrEmpty(s) && !Items.Contains(s))
                    {
                        _ = Items.Add(s);
                        SelectedItem = s;
                    }
                    else
                    {
                        dialogResult = DialogResult.None;
                    }
                }
            }

            private void RemoveItem(object? sender, EventArgs e)
            {
                int i = SelectedIndex;
                Items.RemoveAt(i);
                int n = Items.Count;
                if (n != 0)
                {
                    SelectedIndex = i == n ? i - 1 : i;
                }
            }

            private void ClearItems(object? sender, EventArgs e)
            {
                Items.Clear();
                UpdateEditButtons();
            }

            private async void RenameItemAsync(object? sender, EventArgs e)
            {
                if (OnRenameItem != null)
                {
                    await OnRenameItem.Invoke();
                }
                else
                {
                    int i = SelectedIndex;
                    object? output = Items[i];
                    Map<string, object?> m = new() { { string.Empty, output } };
                    while (output == null || Items.Contains(output))
                    {
                        using InputDialog inputDialog = new(ref m, font: Font);
                        if (inputDialog.ShowDialog() == DialogResult.Cancel)
                        {
                            return;
                        }
                        output = m[string.Empty];
                    }
                    Items[i] = output;
                }
            }

            private void MoveUpItem(object? sender, EventArgs e) => MoveUpDownItem(true);

            private void MoveDownItem(object? sender, EventArgs e) => MoveUpDownItem(false);

            private void SortItems(object? sender, EventArgs e)
            {
                EditListBox.Sorted = true;
                EditListBox.Sorted = false;
                UpdateEditButtons();
            }

            private async void EditItemAsync(object? sender, EventArgs e)
            {
                if (OnEditItem != null)
                {
                    await OnEditItem.Invoke();
                }
            }

            private void UpdateEditButtons()
            {
                bool hasItemSelected = EditListBox.SelectedItems.Count == 1;
                foreach (Button editButton in EditFlowLayoutPanel.GetControls<Button>())
                {
                    switch (editButton.Tag)
                    {
                        case EditButton.Remove:
                        case EditButton.Rename:
                        case EditButton.Sort:
                        case EditButton.Move:
                        case EditButton.Edit:
                            editButton.Enabled = hasItemSelected;
                            break;
                        case EditButton.Clear:
                            editButton.Enabled = Items.Count != 0;
                            break;
                        case EditButton.MoveUp:
                            editButton.Enabled = hasItemSelected & SelectedIndex != 0;
                            break;
                        case EditButton.MoveDown:
                            editButton.Enabled = hasItemSelected & SelectedIndex != Items.Count - 1;
                            break;
                    }
                }
            }

            private void MoveUpDownItem(bool up)
            {
                int i = EditListBox.SelectedIndex;
                object o = EditListBox.Items[i];
                EditListBox.Items.RemoveAt(i);
                i = up ? i - 1 : i + 1;
                EditListBox.Items.Insert(i, o);
                EditListBox.SelectedIndex = i;
            }

            /// <summary>Gets or sets an optional callback that replaces the default add action.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Func<Task>? OnAddItem { get; set; } = null;
            /// <summary>Gets or sets an optional callback that replaces the default rename action.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Func<Task>? OnRenameItem { get; set; } = null;
            /// <summary>Gets or sets the optional callback for editing an item.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Func<Task>? OnEditItem { get; set; } = null;

            /// <summary>Gets the live collection of items being edited.</summary>
            public ListBox.ObjectCollection Items => EditListBox.Items;

            /// <summary>Gets or sets the zero-based index of the selected item, or -1 when nothing is selected.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int SelectedIndex { get => EditListBox.SelectedIndex; set => EditListBox.SelectedIndex = value; }

            /// <summary>Gets or sets the selected item.</summary>
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public object? SelectedItem
            {
                get => SelectedIndex != -1 ? Items[SelectedIndex] : null;
                set { if (value is object o && Items.Count != 0) { EditListBox.SelectedItem = o; } }
            }

            /// <summary>Initializes the dialog with the supplied settings.</summary>
            /// <param name="items">The list that receives the edited items on confirmation.</param>
            /// <param name="caption">The text displayed in the dialog.</param>
            /// <param name="title">The dialog title.</param>
            /// <param name="font">The font to use, or null to use the library default.</param>
            /// <param name="editButtons">The editing buttons to include.</param>
            public EditListDialog
            (
                ref List<object> items,
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null,
                EditButtons editButtons = EditButtons.AddRemoveMoveUpMoveDownSort
            )
            : base(caption, title, font, DialogResultFlags.OKCancel)
            {
                _items = items;

                EditListBox = new()
                {
                    Name = "EditListBox",
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.FixedSingle,
                    IntegralHeight = false,
                    Margin = PADDING_DEFAULT,
                    MinimumSize = BOX_SIZE_MINIMUM,
                    HorizontalScrollbar = true,
                    TabStop = true,
                    TabIndex = 2
                };
                EditFlowLayoutPanel = new()
                {
                    Name = "EditFlowLayoutPanel",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Margin = PADDING_MINIMUM,
                    TabStop = false
                };
                EditTableLayoutPanel = new()
                {
                    Name = "EditTableLayoutPanel",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Margin = PADDING_MINIMUM,
                    TabStop = false
                };
                for (int i = 0; i < EditTableLayoutPanel.ColumnCount; i++)
                {
                    _ = EditTableLayoutPanel.ColumnStyles.Add(new(SizeType.AutoSize));
                }
                for (int i = 0; i < EditTableLayoutPanel.RowCount; i++)
                {
                    _ = EditTableLayoutPanel.RowStyles.Add(new(SizeType.AutoSize));
                }
                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionPanel);
                    CaptionPanel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(EditTableLayoutPanel, 0, 1);
                EditTableLayoutPanel.Controls.Add(EditListBox, 0, 0);
                EditTableLayoutPanel.Controls.Add(EditFlowLayoutPanel, 1, 0);
                EditListBox.SelectedIndexChanged += (sender, e) => UpdateEditButtons();
                EditListBox.Disposed += (sender, e) => EditListBox.ClearEventHandlers();
                foreach (Button editButton in DialogEditButtons.AddButtons(EditFlowLayoutPanel, editButtons))
                {
                    switch (editButton.Tag)
                    {
                        case EditButton.Add:
                            editButton.Click += AddItemAsync;
                            break;
                        case EditButton.Remove:
                            editButton.Click += RemoveItem;
                            break;
                        case EditButton.Rename:
                            editButton.Click += RenameItemAsync;
                            break;
                        case EditButton.MoveUp:
                            editButton.Click += MoveUpItem;
                            break;
                        case EditButton.MoveDown:
                            editButton.Click += MoveDownItem;
                            break;
                        case EditButton.Sort:
                            editButton.Click += SortItems;
                            break;
                        case EditButton.Edit:
                            editButton.Click += EditItemAsync;
                            break;
                        case EditButton.Clear:
                            editButton.Click += ClearItems;
                            break;
                    }
                }
                ActiveControl = EditListBox;
            }
        }

        /// <summary>Displays a countdown and invokes an action when the countdown reaches zero.</summary>
        public class DelayedActionDialog : MessageDialog
        {
            /// <summary>The label that displays the remaining time.</summary>
            protected readonly Label TimerLabel;

            /// <summary>The Windows Forms timer that advances the countdown.</summary>
            protected readonly System.Windows.Forms.Timer _timer;
            /// <summary>The action invoked when the countdown expires.</summary>
            protected readonly Action? _action;
            /// <summary>The number of seconds remaining in the countdown.</summary>
            protected int _numSeconds;

            /// <inheritdoc/>
            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                if (IsDisposed || !Visible || DialogResult != DialogResult.None)
                {
                    return;
                }

                if (_numSeconds == 0)
                {
                    CompleteCountdown();
                }
                else
                {
                    _timer.Start();
                }
            }

            private void Timer_Tick(object? sender, EventArgs e)
            {
                _numSeconds--;
                if (_numSeconds == 0)
                {
                    CompleteCountdown();
                    return;
                }
                TimerLabel.Text = $"{_numSeconds}";
            }

            private void CompleteCountdown()
            {
                _timer.Stop();
                Close();
                _action?.Invoke();
            }

            /// <inheritdoc/>
            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                _timer.Stop();
                base.OnFormClosed(e);
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }
                base.Dispose(disposing);
            }

            /// <summary>Initializes the dialog with the supplied settings.</summary>
            /// <param name="action">The action to invoke when the countdown expires.</param>
            /// <param name="delaySeconds">The initial countdown in seconds. Zero invokes the action as soon as the dialog is shown.</param>
            /// <param name="caption">The text displayed in the dialog.</param>
            /// <param name="title">The dialog title.</param>
            /// <param name="font">The font to use, or null to use the library default.</param>
            /// <remarks>The countdown starts when the dialog is shown.</remarks>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="delaySeconds"/> is negative.</exception>
            public DelayedActionDialog
            (
                Action? action = null,
                int delaySeconds = 10,
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null
            )
            : base(caption, title, font, DialogResultFlags.OKCancel)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(delaySeconds);
                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionPanel);
                    CaptionPanel.Dispose();
                }
                TimerLabel = new()
                {
                    Name = "TimerLabel",
                    Margin = PADDING_DEFAULT,
                    AutoSize = true,
                    Font = new(Font.FontFamily, Font.Size * 2, Font.Style),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Text = $"{delaySeconds}",
                    TabStop = false
                };
                MainTableLayoutPanel.Controls.Add(TimerLabel, 0, 1);
                _numSeconds = delaySeconds;
                _action = action;
                _timer = new() { Interval = 1000 };
                _timer.Tick += Timer_Tick;
            }
        }
    }
}
