namespace WinFormsLib
{

    /// <summary>
    /// Extensible tab page used together with <see cref="AdvTabControl"/>.
    /// </summary>
    public class AdvTabPage : TabPage
    {

        public AdvTabPage() : base()
        {
            InitializeStyles();
        }

        public AdvTabPage(string text) : base(text)
        {
            InitializeStyles();
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                if (base.Enabled != value)
                {
                    base.Enabled = value;
                    InvalidateParentTab();
                }
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            InvalidateParentTab();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            InvalidateParentTab();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            InvalidateParentTab();
        }

        private void InitializeStyles()
        {
            UseVisualStyleBackColor = false;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        private void InvalidateParentTab()
        {
            if (Parent is not null && !Parent.IsDisposed)
            {
                Parent.Invalidate();
            }
        }
    }
}
