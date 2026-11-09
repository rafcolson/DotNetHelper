using System.ComponentModel;

namespace WinFormsLib
{
    /// <summary>
    /// Context menu with internal column metrics initialized at the current DPI.
    /// </summary>
    public sealed class AdvContextMenuStrip : ContextMenuStrip
    {
        /// <summary>
        /// Creates a context menu with DPI-scaled column metrics.
        /// </summary>
        public AdvContextMenuStrip()
        {
            InitializeDpiMetrics();
        }

        /// <summary>
        /// Creates a context menu in the specified component container.
        /// </summary>
        /// <param name="container">The component container that owns the menu.</param>
        public AdvContextMenuStrip(IContainer container) : base(container)
        {
            InitializeDpiMetrics();
        }

        private void InitializeDpiMetrics()
        {
            // Unlike automatically generated menu drop-downs, ContextMenuStrip
            // initializes its internal check/text column metrics at 96 DPI.
            // Initialize those metrics before items and explicit fonts are added.
            if (DeviceDpi != 96)
            {
                RescaleConstantsForDpi(96, DeviceDpi);
            }
        }
    }
}
