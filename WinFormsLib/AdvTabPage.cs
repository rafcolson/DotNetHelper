namespace WinFormsLib
{

    /// <summary>
    /// Extensible tab page used together with <see cref="AdvTabControl"/>.
    /// </summary>
    public class AdvTabPage : TabPage
    {

        /// <summary>Initializes a tab page with custom painting enabled.</summary>
        public AdvTabPage() : base()
        {
            InitializeStyles();
        }

        /// <summary>Initializes a tab page with custom painting enabled and the specified label.</summary>
        /// <param name="text">The text displayed on the tab.</param>
        public AdvTabPage(string text) : base(text)
        {
            InitializeStyles();
        }

        /// <summary>Gets or sets whether the page is enabled. A change requests a redraw of the parent control.</summary>
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

        /// <inheritdoc/>
        /// <remarks>Requests a redraw of the parent control after raising the base event.</remarks>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            InvalidateParentTab();
        }

        /// <inheritdoc/>
        /// <remarks>Requests a redraw of the parent control after raising the base event.</remarks>
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            InvalidateParentTab();
        }

        /// <inheritdoc/>
        /// <remarks>Requests a redraw of the parent control after raising the base event.</remarks>
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
