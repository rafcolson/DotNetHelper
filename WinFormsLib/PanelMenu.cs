using static WinFormsLib.Constants;

namespace WinFormsLib
{
    /// <summary>A button that opens a hierarchy of panels on its containing form.</summary>
    public class PanelMenu : Button
    {

        private readonly Dictionary<string, Panel> panels = [];
        private readonly List<string> _panelsActive = [];

        private Form? form;
        private bool menuMouseHover = false;
        private EventArgs? menuEventArgs = null;
        private Size _buttonSize = new(40, 40);
        private Color _buttonActiveForeColor;
        private Color _buttonActiveBackColor;
        private readonly System.Windows.Forms.Timer AllMouseLeaveTimer;

        /// <summary>Occurs after the pointer has left the menu and the leave delay expires.</summary>
        public event MenuMouseLeaveEventHandler? MenuMouseLeave;

        /// <summary>Handles a delayed menu leave notification.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        public delegate void MenuMouseLeaveEventHandler(object? sender, EventArgs e);
        /// <summary>Occurs when the pointer enters the menu.</summary>
        public event MenuMouseEnterEventHandler? MenuMouseEnter;

        /// <summary>Handles a menu enter notification.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        public delegate void MenuMouseEnterEventHandler(object? sender, EventArgs e);
        /// <summary>Occurs when a panel button receives a mouse click.</summary>
        public event PanelButtonClickEventHandler? PanelButtonClick;

        /// <summary>Handles a mouse click on a panel button.</summary>
        /// <param name="button">The panel button that was clicked.</param>
        /// <param name="e">The event data.</param>
        public delegate void PanelButtonClickEventHandler(PanelButton button, MouseEventArgs e);

        /// <summary>Gets a snapshot of the active panel names in opening order.</summary>
        public string[] PanelsActive
        {
            get
            {
                return [.. _panelsActive];
            }
        }

        /// <summary>Gets or sets the maximum row count used when filling panels by columns.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public int RowCountMax
        {
            get;
            set
            {
                field = Math.Max(value, 1);
            }
        } = 10;

        /// <summary>Gets or sets the maximum column count used when filling panels by rows.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public int ColumnCountMax
        {
            get;
            set
            {
                field = Math.Max(value, 1);
            }
        } = 10;

        /// <summary>Gets or sets the logical size used for panel buttons at 96 DPI.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Size ButtonSize
        {
            get
            {
                return _buttonSize;
            }
            set
            {
                _buttonSize = value;
            }
        }

        /// <summary>Gets or sets the mouse leave delay in milliseconds, with a minimum of one.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public int MouseLeaveDelay
        {
            get => AllMouseLeaveTimer.Interval;
            set => AllMouseLeaveTimer.Interval = Math.Max(value, 1);
        }

        internal Size ScaledButtonSize
        {
            get
            {
                return new Size(Math.Max((int)Math.Round(Math.Round(_buttonSize.Width * DeviceDpi / 96.0d)), 1), Math.Max((int)Math.Round(Math.Round(_buttonSize.Height * DeviceDpi / 96.0d)), 1));
            }
        }

        /// <summary>Gets or sets the foreground color assigned to active panel buttons.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color ButtonActiveForeColor
        {
            get
            {
                return _buttonActiveForeColor;
            }
            set
            {
                _buttonActiveForeColor = value;
                foreach (Panel p in panels.Values)
                {
                    foreach (PanelButton b in p.Controls)
                    {
                        b.ActiveForeColor = value;
                    }
                }
            }
        }

        /// <summary>Gets or sets the background color assigned to active panel buttons.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color ButtonActiveBackColor
        {
            get
            {
                return _buttonActiveBackColor;
            }
            set
            {
                _buttonActiveBackColor = value;
                foreach (Panel p in panels.Values)
                {
                    foreach (PanelButton b in p.Controls)
                    {
                        b.ActiveBackColor = value;
                    }
                }
            }
        }

        /// <summary>Gets or sets the panel definitions and rebuilds the menu when assigned.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Level[] Levels
        {
            get;
            set
            {
                field = value;
                Init();
            }
        } = [];

        /// <summary>Gets or sets the font used when panel button appearance is applied.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Font ButtonFont { get; set; }

        /// <summary>Updates the size and font of existing panel buttons and repositions open panels.</summary>
        /// <param name="buttonSize">The button size at 96 DPI.</param>
        /// <param name="buttonFont">The font for the panel buttons.</param>
        public void SetButtonAppearance(Size buttonSize, Font buttonFont)
        {
            ArgumentNullException.ThrowIfNull(buttonFont);
            _buttonSize = buttonSize;
            ButtonFont = buttonFont;

            foreach (Panel panel in panels.Values)
            {
                foreach (PanelButton button in panel.Controls)
                {
                    button.UpdateAppearance();
                }
                panel.PerformLayout();
            }

            foreach (string panelName in _panelsActive)
            {
                panels[panelName].UpdateLocation();
            }
        }

        /// <summary>Gets or sets whether the menu uses collapsing behavior.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public bool UseCollapsing { get; set; } = false;

        /// <summary>Gets or sets whether items fill columns first. Changing this value rebuilds the menu.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public bool FillByColumns
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    Init();
                }
            }
        }

        /// <summary>Gets or sets the panel names used when reopening the last selection.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public string[] LastSelection { get; set; } = [];

        /// <summary>Gets or sets the horizontal alignment of the menu panels.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Align Alignment { get; set; } = Align.Left;

        /// <summary>Specifies a horizontal panel alignment.</summary>
        public enum Align : int
        {
            /// <summary>Aligns panels to the left.</summary>
            Left = 0,
            /// <summary>Aligns panels to the right.</summary>
            Right = 1
        }

        /// <summary>Defines an item in a menu level.</summary>
        /// <param name="name">The identifier.</param>
        /// <param name="text">The text to display.</param>
        /// <param name="child">The identifier of the child panel, if any.</param>
        /// <param name="enabled">Whether the item is enabled.</param>
        public class Item(string? name = null, string? text = null, string? child = null, bool enabled = true)
        {
            /// <summary>Gets or sets the identifier.</summary>
            public string Name { get; set; } = string.IsNullOrEmpty(name) ? string.Empty : name;
            /// <summary>Gets or sets the text displayed on the item.</summary>
            public string Text { get; set; } = string.IsNullOrEmpty(text) ? string.Empty : text;
            /// <summary>Gets or sets the identifier of the child panel.</summary>
            public string Target { get; set; } = string.IsNullOrEmpty(child) ? string.Empty : child;
            /// <summary>Gets or sets whether the item is enabled.</summary>
            public bool Enabled { get; set; } = enabled;
        }

        /// <summary>Defines a panel and the items it contains.</summary>
        /// <param name="name">The identifier.</param>
        /// <param name="parent">The identifier of the parent panel, if any.</param>
        /// <param name="items">The items to display.</param>
        public class Level(string? name = null, string? parent = null, PanelMenu.Item[]? items = null)
        {
            /// <summary>Gets or sets the parent panel identifier.</summary>
            public string Parent { get; set; } = string.IsNullOrEmpty(parent) ? string.Empty : parent;
            /// <summary>Gets or sets the identifier.</summary>
            public string Name { get; set; } = string.IsNullOrEmpty(name) ? string.Empty : name;
            /// <summary>Gets or sets the items displayed in the panel.</summary>
            public Item[] Items { get; set; } = items ?? [];

            /// <summary>Initializes an empty panel definition.</summary>
            public Level() : this(null, null, null)
            {
            }
        }

        /// <summary>Represents a button inside a menu panel.</summary>
        public class PanelButton : Button
        {

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            internal Color ActiveForeColor { get; set; }
            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            internal Color ActiveBackColor { get; set; }
            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            internal Color InActiveForeColor { get; set; }
            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            internal Color InActiveBackColor { get; set; }
            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            internal string PanelName { get; set; }

            private PanelMenu Root { get; set; }

            /// <summary>Initializes a button for the specified panel.</summary>
            /// <param name="parentPanel">The panel containing the button.</param>
            /// <param name="name">The identifier.</param>
            /// <param name="text">The text to display.</param>
            /// <param name="panelName">The panel identifier.</param>
            public PanelButton(Panel parentPanel, string name, string text, string panelName) : base()
            {
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                Name = name;
                Text = text;
                PanelName = panelName;
                Root = parentPanel.Root;
                ForeColor = Root.ForeColor;
                BackColor = Root.BackColor;
                InActiveForeColor = Root.ForeColor;
                InActiveBackColor = Root.BackColor;
                ActiveForeColor = Root.ButtonActiveForeColor;
                ActiveBackColor = Root.ButtonActiveBackColor;
                UpdateAppearance();
                MouseEnter += On_MouseEnter;
                MouseLeave += On_MouseLeave;
            }

            internal void UpdateAppearance()
            {
                int scaledMargin = Math.Max((int)Math.Round(Math.Round(Root.DeviceDpi / 96.0d)), 1);
                Margin = new Padding(scaledMargin);
                Width = Root.ScaledButtonSize.Width - (scaledMargin * 2);
                Height = Root.ScaledButtonSize.Height - (scaledMargin * 2);
                Font = Root.ButtonFont;
            }

            /// <summary>Applies the active or inactive colors to the button.</summary>
            /// <param name="isActive">Whether to apply the active colors.</param>
            public void Active(bool isActive)
            {
                if (isActive)
                {
                    ForeColor = ActiveForeColor;
                    BackColor = ActiveBackColor;
                }
                else
                {
                    ForeColor = InActiveForeColor;
                    BackColor = InActiveBackColor;
                }
            }

            private void On_MouseEnter(object? sender, EventArgs e)
            {
                Root.AllMouseEnter(e);
            }

            private void On_MouseLeave(object? sender, EventArgs e)
            {
                Root.AllMouseLeave(e);
            }

        }

        /// <summary>Represents a menu panel containing buttons in a table layout.</summary>
        public class Panel : TableLayoutPanel
        {

            internal PanelMenu Root { get; private set; }
            internal Panel? ParentPanel { get; private set; }
            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            internal PanelButton? ActiveButton { get; set; } = null;

            internal Panel(PanelMenu root, Panel? parent, string name, int rowCount, int columnCount) : base()
            {
                AutoSize = true;
                AutoSizeMode = AutoSizeMode.GrowAndShrink;
                BorderStyle = BorderStyle.None;
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
                DoubleBuffered = true;
                Margin = new Padding(0);
                Root = root;
                ParentPanel = parent;
                Name = name;
                RowCount = rowCount;
                ColumnCount = columnCount;
                ForeColor = root.ForeColor;
                BackColor = root.BackColor;
                MouseEnter += On_MouseEnter;
                MouseLeave += On_MouseLeave;
                Paint += On_Paint;
            }

            internal void UpdateLocation()
            {
                Point p = new();
                if (ParentPanel == null)
                {
                    p = GetLocation(Root);
                    p.Y += Root.Height;
                }
                else
                {
                    p.Y += ParentPanel.Top;
                }
                if (Root.Alignment.Equals(Align.Right))
                {
                    p.X += (ParentPanel == null ? Root.Width : ParentPanel.Left) - (Root.ScaledButtonSize.Width * ColumnCount);
                }
                else if (Root.Alignment.Equals(Align.Left) && !(ParentPanel == null))
                {
                    p.X += ParentPanel.Left + (Root.ScaledButtonSize.Width * ParentPanel.ColumnCount);
                }
                Location = p;
            }

            internal void AddButton(string name, string text, string panelName, bool enabled, int index, int itemCount)
            {
                PanelButton pb = new(this, name, text, panelName) { Enabled = enabled };
                pb.Active(false);
                pb.MouseClick += On_PanelButton_Click;
                if (Root.FillByColumns)
                {
                    int effectiveRowCount = Math.Max((int)Math.Ceiling(itemCount / (double)ColumnCount), 1);
                    RowCount = effectiveRowCount;
                    Controls.Add(pb, index / effectiveRowCount, index % effectiveRowCount);
                }
                else
                {
                    Controls.Add(pb);
                }
            }

            private void On_PanelButton_Click(object? sender, MouseEventArgs e)
            {
                if (sender is object o)
                {
                    PanelButton pb = (PanelButton)o;
                    if (!string.IsNullOrEmpty(pb.PanelName))
                    {
                        if (Root.panels.ContainsKey(pb.PanelName))
                        {
                            if (!Root.Opened(pb.PanelName))
                            {
                                Panel panel = Root.panels[pb.PanelName];
                                if (!(panel.ParentPanel == null))
                                {
                                    if (!(panel.ParentPanel.ActiveButton == null))
                                    {
                                        Root.Close(panel.ParentPanel.ActiveButton.PanelName);
                                    }
                                }
                                Root.Open(pb.PanelName);
                            }
                        }
                        else
                        {
                            _ = MessageBox.Show("Target level with name '" + pb.PanelName + "' does not exist.");
                        }
                    }
                    else if (!Root.panels.ContainsKey(pb.Name))
                    {
                        List<string> l = [pb.Name];
                        Control? c = pb.Parent;
                        while (c != null)
                        {
                            Panel p = (Panel)c;
                            l.Add(p.Name);
                            c = p.ParentPanel;
                        }
                        l.Reverse();
                        Root.LastSelection = [.. l];
                        Root.RaisePanelButtonClick(pb, e);
                        Root.Close();
                    }
                    else
                    {
                        _ = MessageBox.Show("No target assigned to level with name '" + pb.Name + "'.");
                    }
                }

            }

            private void On_MouseEnter(object? sender, EventArgs e)
            {
                Root.AllMouseEnter(e);
            }

            private void On_MouseLeave(object? sender, EventArgs e)
            {
                Root.AllMouseLeave(e);
            }

            private void On_Paint(object? sender, PaintEventArgs e)
            {
                Rectangle borderRectangle = ClientRectangle;
                borderRectangle.Inflate(1, 1);
                borderRectangle.Offset(-1, -1);
                ControlPaint.DrawBorder3D(e.Graphics, borderRectangle, Border3DStyle.RaisedOuter);
            }

        }

        /// <summary>Initializes a menu button.</summary>
        public PanelMenu()
        {
            _buttonActiveForeColor = ForeColor;
            _buttonActiveBackColor = BackColor;
            ButtonFont = Font;
            AllMouseLeaveTimer = new System.Windows.Forms.Timer();
            MouseLeaveDelay = PANEL_MENU_MOUSE_LEAVE_DELAY;
            MouseEnter += On_MouseEnter;
            MouseLeave += On_MouseLeave;
            ForeColorChanged += On_ForeColorChanged;
            BackColorChanged += On_BackColorChanged;
            Click += On_Click;
            AllMouseLeaveTimer.Tick += AllMouseLeaveTimer_Tick;
        }

        /// <inheritdoc/>
        protected override void CreateHandle()
        {
            base.CreateHandle();
            form = FindForm();
            Init();
        }

        private static Point GetLocation(Control control)
        {
            Point point = new();
            while (!(control.Parent == null))
            {
                point.X += control.Left;
                point.Y += control.Top;
                control = control.Parent;
            }
            return point;
        }

        private void Init()
        {
            DisposePanels();
            foreach (Level l in Levels)
            {
                int n = l.Items.Length;
                int x = (int)Math.Round(Math.Max(Math.Min(n / (double)ColumnCountMax, RowCountMax), 1d));
                int y = (int)Math.Round(Math.Max(Math.Min(n / (double)RowCountMax, ColumnCountMax), 1d));
                Panel? pp = null;
                if (!string.IsNullOrEmpty(l.Parent) && panels.TryGetValue(l.Parent, out Panel? value))
                {
                    pp = value;
                }
                Panel p = new(this, pp, l.Name, x, y);
                for (int index = 0; index < l.Items.Length; index++)
                {
                    Item it = l.Items[index];
                    p.AddButton(it.Name, it.Text, it.Target, it.Enabled, index, l.Items.Length);
                }

                panels.Add(l.Name, p);
            }
        }

        private void DisposePanels()
        {
            AllMouseLeaveTimer.Stop();
            foreach (Panel panel in panels.Values)
            {
                panel.Dispose();
            }
            panels.Clear();
            _panelsActive.Clear();
            menuMouseHover = false;
            menuEventArgs = null;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposePanels();
                AllMouseLeaveTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        private void RaisePanelButtonClick(PanelButton pb, MouseEventArgs e)
        {
            PanelButtonClick?.Invoke(pb, e);
        }

        private void AllMouseEnter(EventArgs e)
        {
            AllMouseLeaveTimer.Stop();
            if (!menuMouseHover)
            {
                menuMouseHover = true;
                if (menuEventArgs == null)
                {
                    MenuMouseEnter?.Invoke(this, e);
                }
            }
            menuEventArgs = null;
        }

        private void AllMouseLeave(EventArgs e)
        {
            menuMouseHover = false;
            menuEventArgs = e;
            AllMouseLeaveTimer.Stop();
            AllMouseLeaveTimer.Start();
        }

        private void AllMouseLeaveTimer_Tick(object? sender, EventArgs e)
        {
            if (!menuMouseHover)
            {
                AllMouseLeaveTimer.Stop();
                if (menuEventArgs is EventArgs eventArgs)
                {
                    MenuMouseLeave?.Invoke(this, eventArgs);
                }
                menuEventArgs = null;
                if (Opened())
                {
                    Close();
                }
            }
        }

        private void On_MouseEnter(object? sender, EventArgs e)
        {
            AllMouseEnter(e);
        }

        private void On_MouseLeave(object? sender, EventArgs e)
        {
            AllMouseLeave(e);
        }

        private void On_ForeColorChanged(object? sender, EventArgs e)
        {
            foreach (Panel p in panels.Values)
            {
                p.ForeColor = ForeColor;
                foreach (PanelButton b in p.Controls)
                {
                    b.ForeColor = ForeColor;
                    b.InActiveForeColor = ForeColor;
                }
            }
        }

        private void On_BackColorChanged(object? sender, EventArgs e)
        {
            foreach (Panel p in panels.Values)
            {
                p.BackColor = BackColor;
                foreach (PanelButton b in p.Controls)
                {
                    b.BackColor = BackColor;
                    b.InActiveBackColor = BackColor;
                }
            }
        }

        private void On_Click(object? sender, EventArgs e)
        {
            if (Opened())
            {
                Close();
            }
            else if (UseCollapsing)
            {
                OpenLast();
            }
            else
            {
                Open();
            }
        }

        /// <summary>Opens the specified panel on the containing form.</summary>
        /// <param name="panelName">The panel identifier, or null or empty to use the first panel.</param>
        /// <remarks>Opening an already active panel has no effect.</remarks>
        /// <exception cref="InvalidOperationException">The menu has no containing form, has no default panel, or the parent button cannot be found.</exception>
        /// <exception cref="KeyNotFoundException">The specified panel does not exist.</exception>
        public void Open(string? panelName = null)
        {
            Form ownerForm = form ?? FindForm()
                ?? throw new InvalidOperationException("PanelMenu must be added to a form before a panel can be opened.");
            form = ownerForm;
            if (string.IsNullOrEmpty(panelName))
            {
                panelName = panels.First().Key;
            }
            if (_panelsActive.Contains(panelName))
            {
                return;
            }

            Panel panel = panels[panelName];
            if (panel.ParentPanel is Panel parentPanel)
            {
                if (parentPanel.ActiveButton is not null)
                {
                    Close(parentPanel.ActiveButton.PanelName);
                }
                if (parentPanel.Controls[panelName] is not PanelButton activeButton)
                {
                    throw new InvalidOperationException($"Panel button '{panelName}' was not found in parent panel '{parentPanel.Name}'.");
                }
                parentPanel.ActiveButton = activeButton;
                activeButton.Active(true);
            }
            panel.UpdateLocation();
            ownerForm.Controls.Add(panel);
            panel.BringToFront();
            _panelsActive.Add(panelName);
        }

        /// <summary>Reopens known panels from the last selection, or opens the first panel if the selection is empty.</summary>
        public void OpenLast()
        {
            if (LastSelection.Length != 0)
            {
                foreach (string panelName in LastSelection)
                {
                    if (panels.ContainsKey(panelName))
                    {
                        Open(panelName);
                    }
                }
            }
            else if (panels.Count != 0)
            {
                Open();
            }
        }

        /// <summary>Closes the specified panel and its active descendants.</summary>
        /// <param name="panelName">The panel identifier, or null or empty to use the first panel.</param>
        /// <exception cref="InvalidOperationException">No panel is available when the default panel is requested.</exception>
        /// <exception cref="KeyNotFoundException">The specified panel does not exist.</exception>
        public void Close(string? panelName = null)
        {
            if (string.IsNullOrEmpty(panelName))
            {
                panelName = panels.First().Key;
            }
            Panel panel = panels[panelName];
            if (!(panel.ActiveButton == null))
            {
                Close(panel.ActiveButton.PanelName);
            }
            if (panel.ParentPanel is Panel parentPanel)
            {
                parentPanel.ActiveButton?.Active(false);
                parentPanel.ActiveButton = null;
            }
            form?.Controls.Remove(panel);
            _ = _panelsActive.Remove(panelName);
        }

        /// <summary>Determines whether the specified panel is open.</summary>
        /// <param name="panelName">The panel identifier, or null or empty to use the first panel.</param>
        public bool Opened(string? panelName = null)
        {
            if (panels.Count != 0)
            {
                if (string.IsNullOrEmpty(panelName))
                {
                    panelName = panels.First().Key;
                }
                return _panelsActive.Contains(panelName);
            }
            return false;
        }

    }
}
