using static WinFormsLib.Constants;

namespace WinFormsLib
{
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

        public event MenuMouseLeaveEventHandler? MenuMouseLeave;

        public delegate void MenuMouseLeaveEventHandler(object? sender, EventArgs e);
        public event MenuMouseEnterEventHandler? MenuMouseEnter;

        public delegate void MenuMouseEnterEventHandler(object? sender, EventArgs e);
        public event PanelButtonClickEventHandler? PanelButtonClick;

        public delegate void PanelButtonClickEventHandler(PanelButton button, MouseEventArgs e);

        public string[] PanelsActive
        {
            get
            {
                return [.. _panelsActive];
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public int RowCountMax
        {
            get;
            set
            {
                field = Math.Max(value, 1);
            }
        } = 10;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public int ColumnCountMax
        {
            get;
            set
            {
                field = Math.Max(value, 1);
            }
        } = 10;

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

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Font ButtonFont { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public bool UseCollapsing { get; set; } = false;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public string[] LastSelection { get; set; } = [];

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Align Alignment { get; set; } = Align.Left;

        public enum Align : int
        {
            Left = 0,
            Right = 1
        }

        public class Item(string? name = null, string? text = null, string? child = null)
        {
            public string Name { get; set; } = string.IsNullOrEmpty(name) ? string.Empty : name;
            public string Text { get; set; } = string.IsNullOrEmpty(text) ? string.Empty : text;
            public string Target { get; set; } = string.IsNullOrEmpty(child) ? string.Empty : child;
        }

        public class Level(string? name = null, string? parent = null, PanelMenu.Item[]? items = null)
        {
            public string Parent { get; set; } = string.IsNullOrEmpty(parent) ? string.Empty : parent;
            public string Name { get; set; } = string.IsNullOrEmpty(name) ? string.Empty : name;
            public Item[] Items { get; set; } = items ?? [];

            public Level() : this(null, null, null)
            {
            }
        }

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

            public PanelButton(Panel parentPanel, string name, string text, string panelName) : base()
            {
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                int scaledMargin = Math.Max((int)Math.Round(Math.Round(RootDpi(parentPanel) / 96.0d)), 1);
                Margin = new Padding(scaledMargin);
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
                Width = Root.ScaledButtonSize.Width - (scaledMargin * 2);
                Height = Root.ScaledButtonSize.Height - (scaledMargin * 2);
                Font = Root.ButtonFont;
                MouseEnter += On_MouseEnter;
                MouseLeave += On_MouseLeave;
            }

            private static int RootDpi(Panel parentPanel)
            {
                return parentPanel.Root.DeviceDpi;
            }

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

            internal void AddButton(string name, string text, string panelName)
            {
                PanelButton pb = new(this, name, text, panelName);
                pb.Active(false);
                pb.MouseClick += On_PanelButton_Click;
                Controls.Add(pb);
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
                foreach (Item it in l.Items)
                {
                    p.AddButton(it.Name, it.Text, it.Target);
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

        public void Open(string? panelName = null)
        {
            Form ownerForm = form ?? FindForm()
                ?? throw new InvalidOperationException("PanelMenu must be added to a form before a panel can be opened.");
            form = ownerForm;
            if (string.IsNullOrEmpty(panelName))
            {
                panelName = panels.First().Key;
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
