using System.Diagnostics;
using System.ComponentModel;

using static WinFormsLib.ToolStripMenuItems;
using static WinFormsLib.Constants;
using static WinFormsLib.Buttons;
using static WinFormsLib.Utils;

namespace WinFormsLib
{
    public static class Forms
    {
        public class EditTextContextMenuStrip : ContextMenuStrip
        {
            protected override void Dispose(bool disposing)
            {
                ToolStripMenuItem[] items = Items.Cast<ToolStripMenuItem>().ToArray();
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
                                tsmi.Enabled = tb.Focused && tb.Text.Any();
                                break;
                        }
                    }
                }
            }

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

        public class MessageDialog : Form
        {
            protected readonly LinkLabel CaptionLabel = new();
            protected readonly FlowLayoutPanel DialogFlowLayoutPanel = new();
            protected readonly TableLayoutPanel MainTableLayoutPanel = new();

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                this.CenterOnScreen();
            }
            protected override void Dispose(bool disposing)
            {
                this.Clear();
                base.Dispose(disposing);
            }

            private void InitializeComponent()
            {
                SuspendLayout();
                CaptionLabel.SuspendLayout();
                DialogFlowLayoutPanel.SuspendLayout();
                MainTableLayoutPanel.SuspendLayout();

                CaptionLabel.Name = "CaptionLabel";
                CaptionLabel.Dock = DockStyle.Fill;
                CaptionLabel.Margin = PADDING_MINIMUM;
                CaptionLabel.LinkBehavior = LinkBehavior.HoverUnderline;
                CaptionLabel.TabStop = false;

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
                MainTableLayoutPanel.Padding = PADDING_MINIMUM;
                MainTableLayoutPanel.ColumnCount = 1;
                MainTableLayoutPanel.ColumnStyles.Add(new(SizeType.AutoSize));
                MainTableLayoutPanel.RowCount = 3;
                for (int i = 0; i < MainTableLayoutPanel.RowCount; i++)
                {
                    MainTableLayoutPanel.RowStyles.Add(new(SizeType.AutoSize));
                }
                MainTableLayoutPanel.TabStop = false;

                Name = "MainDialog";
                AutoSize = true;
                AutoSizeMode = AutoSizeMode.GrowAndShrink;
                AutoScaleMode = AutoScaleMode.Font;
                SizeGripStyle = SizeGripStyle.Hide;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowIcon = false;
                ShowInTaskbar = false;
                TopMost = true;

                Controls.Add(MainTableLayoutPanel);
                MainTableLayoutPanel.Controls.Add(CaptionLabel, 0, 0);
                MainTableLayoutPanel.Controls.Add(DialogFlowLayoutPanel, 0, 2);

                CaptionLabel.ResumeLayout();
                DialogFlowLayoutPanel.ResumeLayout();
                MainTableLayoutPanel.ResumeLayout();
                MainTableLayoutPanel.PerformLayout();
                ResumeLayout();
                PerformLayout();
            }

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
                CaptionLabel.MinimumSize = CalculateSize(caption, font, padding: CaptionLabel.Padding);
                DialogResultButtons.AddButtons(DialogFlowLayoutPanel, buttons);
            }
        }

        public class MessageCheckDialog : MessageDialog
        {
            protected readonly CheckBox CheckBox;

            public bool Checked { get; set; }

            private void MessageCheckBox_CheckChanged(object? sender, EventArgs e) => Checked = CheckBox.Checked;

            public MessageCheckDialog
            (
                string checkText,
                string caption = EMPTY_STRING,
                string title = EMPTY_STRING,
                Font? font = null
            )
            : base(caption, title, font, DialogResultFlags.YesNo)
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
                CheckBox.CheckedChanged += MessageCheckBox_CheckChanged;
                CheckBox.Disposed += (object? sender, EventArgs e) => CheckBox.CheckedChanged -= MessageCheckBox_CheckChanged;

                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionLabel);
                    CaptionLabel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(CheckBox, 0, 1);
            }
        }

        public class ImageDialog : MessageDialog
        {
            protected readonly PictureBox PictureBox;

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
                    MainTableLayoutPanel.Controls.Remove(CaptionLabel);
                    CaptionLabel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(PictureBox, 0, 1);
            }
        }

        public class ImageTableDialog : MessageDialog
        {
            protected readonly TableLayoutPanel ImageTableLayoutPanel;

            private readonly List<Point> _imagePositions = new();

            public int SelectedIndex { get; private set; }
            public string CaptionText { get => CaptionLabel.Text; set => CaptionLabel.Text = value; }

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
                        PictureBox pb = (PictureBox)ImageTableLayoutPanel.GetControlFromPosition(p.X, p.Y);
                        pb.BackColor = SelectedIndex == i ? SystemColors.ControlLightLight : SystemColors.Control;
                        pb.BorderStyle = SelectedIndex == i ? BorderStyle.Fixed3D : BorderStyle.None;
                    }
                    ImageTableLayoutPanel.ResumeDrawing();
                    OnSelectedIndexChanged?.Invoke();
                }
            }

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
                int w = (preferredSize.Width - 24 - margin * 6) / cc;
                int h = (preferredSize.Height - 192 - margin * 6) / rc;
                for (int ic = 0; ic < cc; ic++)
                {
                    ImageTableLayoutPanel.ColumnStyles.Add(new(SizeType.Absolute, w));
                }
                for (int ir = 0; ir < rc; ir++)
                {
                    ImageTableLayoutPanel.RowStyles.Add(new(SizeType.Absolute, h));
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

        public class DropDownListDialog : MessageDialog
        {
            protected readonly ComboBox DropDownList;

            public int SelectedIndex { get => DropDownList.SelectedIndex; set => DropDownList.SelectedIndex = value; }
            public object SelectedItem { get => DropDownList.SelectedItem; set => DropDownList.SelectedItem = value; }

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
                    MainTableLayoutPanel.Controls.Remove(CaptionLabel);
                    CaptionLabel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(DropDownList, 0, 1);
                SelectedIndex = selectedIndex;
            }
        }

        public class InputDialog : MessageDialog
        {
            protected readonly TableLayoutPanel InputTableLayoutPanel;

            protected readonly Map<string, object?> _input;

            protected override void OnClosed(EventArgs e)
            {
                if (DialogResult == AcceptButton.DialogResult)
                {
                    for (int i = 0; i < InputTableLayoutPanel.RowCount; i++)
                    {
                        string k = InputTableLayoutPanel.GetControlFromPosition(0, i).Text;
                        string v = InputTableLayoutPanel.GetControlFromPosition(1, i).Text;
                        _input[k] = v;
                    }
                    OnUpdate?.Invoke();
                }
                base.OnClosed(e);
            }

            private void TextBox_MouseDown(object? sender, MouseEventArgs e)
            {
                if (sender is TextBox tb && e.Button == MouseButtons.Right)
                {
                    tb.Focus();
                }
            }

            public Func<Task>? OnUpdate { get; set; } = null;

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
                KeyValuePair<string, object?>[] kvpa = _input.ToArray();
                ColumnStyle cs = kvpa.Length == 1 && string.IsNullOrEmpty(kvpa[0].Key) ? new(SizeType.Absolute, 1) : new(SizeType.AutoSize);
                InputTableLayoutPanel.ColumnStyles.Add(cs);
                InputTableLayoutPanel.ColumnStyles.Add(new(SizeType.Percent, 100F));
                for (int i = 0; i < InputTableLayoutPanel.RowCount; i++)
                {
                    InputTableLayoutPanel.RowStyles.Add(new(SizeType.AutoSize));
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
                    tb.Disposed += (object? sender, EventArgs e) => tb.MouseDown -= TextBox_MouseDown;
                    InputTableLayoutPanel.Controls.Add(l, 0, i);
                    InputTableLayoutPanel.Controls.Add(tb, 1, i);
                }

                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionLabel);
                    CaptionLabel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(InputTableLayoutPanel, 0, 1);
                if (InputTableLayoutPanel.GetControlFromPosition(1, 0) is TextBox tb1)
                {
                    ActiveControl = tb1;
                }
            }
        }

        public class EditListDialog : MessageDialog
        {
            protected readonly TableLayoutPanel EditTableLayoutPanel;
            protected readonly FlowLayoutPanel EditFlowLayoutPanel;
            protected readonly ListBox EditListBox;

            private readonly List<object> _items;

            protected override void OnShown(EventArgs e)
            {
                Items.Clear();
                Items.AddRange(_items.ToArray());
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

            protected override void OnClosed(EventArgs e)
            {
                if (DialogResult == AcceptButton.DialogResult)
                {
                    _items.Replace(Items.Cast<object>());
                }
                base.OnClosed(e);
            }

            private void AddItem(object? sender, EventArgs e)
            {
                if (OnAddItem != null)
                {
                    OnAddItem.Invoke();
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
                        Items.Add(s);
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

            private void RenameItem(object? sender, EventArgs e)
            {
                if (OnRenameItem != null)
                {
                    OnRenameItem.Invoke();
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

            private void EditItem(object? sender, EventArgs e) => OnEditItem?.Invoke();

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

            public Func<Task>? OnAddItem { get; set; } = null;
            public Func<Task>? OnRenameItem { get; set; } = null;
            public Func<Task>? OnEditItem { get; set; } = null;

            public ListBox.ObjectCollection Items => EditListBox.Items;

            public int SelectedIndex { get => EditListBox.SelectedIndex; set => EditListBox.SelectedIndex = value; }

            public object? SelectedItem
            {
                get => SelectedIndex != -1 ? Items[SelectedIndex] : null;
                set { if (value is object o && Items.Count != 0) { EditListBox.SelectedItem = o; } }
            }

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
                    EditTableLayoutPanel.ColumnStyles.Add(new(SizeType.AutoSize));
                }
                for (int i = 0; i < EditTableLayoutPanel.RowCount; i++)
                {
                    EditTableLayoutPanel.RowStyles.Add(new(SizeType.AutoSize));
                }
                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionLabel);
                    CaptionLabel.Dispose();
                }
                MainTableLayoutPanel.Controls.Add(EditTableLayoutPanel, 0, 1);
                EditTableLayoutPanel.Controls.Add(EditListBox, 0, 0);
                EditTableLayoutPanel.Controls.Add(EditFlowLayoutPanel, 1, 0);
                EditListBox.SelectedIndexChanged += (object? sender, EventArgs e) => UpdateEditButtons();
                EditListBox.Disposed += (object? sender, EventArgs e) => EditListBox.ClearEventHandlers();
                foreach (Button editButton in DialogEditButtons.AddButtons(EditFlowLayoutPanel, editButtons))
                {
                    switch (editButton.Tag)
                    {
                        case EditButton.Add:
                            editButton.Click += AddItem;
                            break;
                        case EditButton.Remove:
                            editButton.Click += RemoveItem;
                            break;
                        case EditButton.Rename:
                            editButton.Click += RenameItem;
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
                            editButton.Click += EditItem;
                            break;
                    }
                }
                ActiveControl = EditListBox;
            }
        }

        public class DelayedActionDialog : MessageDialog
        {
            protected readonly Label TimerLabel;

            protected readonly System.Windows.Forms.Timer _timer;
            protected readonly Action? _action;
            protected int _numSeconds;

            private void Timer_Tick(object? sender, EventArgs e)
            {
                _numSeconds--;
                if (_numSeconds == 0)
                {
                    Close();
                    _action?.Invoke();
                    return;
                }
                TimerLabel.Text = $"{_numSeconds}";
            }
            private void DelayedActionDialog_Closing(object? sender, CancelEventArgs e)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer.Dispose();
                if (sender is DelayedActionDialog dad)
                {
                    dad.ClearEventHandlers();
                }
            }

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
                if (string.IsNullOrEmpty(caption))
                {
                    MainTableLayoutPanel.Controls.Remove(CaptionLabel);
                    CaptionLabel.Dispose();
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
                Closing += DelayedActionDialog_Closing;
                _numSeconds = delaySeconds;
                _action = action;
                _timer = new() { Interval = 1000, Enabled = true };
                _timer.Tick += Timer_Tick;
            }
        }
    }
}
