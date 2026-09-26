namespace WinFormsLib
{
    /// <summary>
    /// SplitContainer that buffers painting of its panels and child windows together.
    /// </summary>
    public sealed class AdvSplitContainer : SplitContainer
    {
        private const int WS_EX_COMPOSITED = 0x02000000;

        /// <inheritdoc />
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                // Buffer the panels and their child windows together during layout changes.
                parameters.ExStyle |= WS_EX_COMPOSITED;
                return parameters;
            }
        }
    }
}
