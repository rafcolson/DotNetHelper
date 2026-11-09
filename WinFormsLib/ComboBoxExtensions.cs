namespace WinFormsLib
{
    /// <summary>Provides layout helpers for combo boxes.</summary>
    public static class ComboBoxExtensions
    {
        /// <summary>Sets the drop-down width based on the items' string representations measured in the control's font.</summary>
        /// <param name="super">The combo box to resize.</param>
        public static void UpdateDropDownList(this ComboBox super)
        {
            DockStyle ds = super.Dock;
            super.Dock = DockStyle.None;
            super.DropDownWidth = Utils.GetMaxWidth([.. super.Items.Cast<object>()], super.Font);
            super.Dock = ds;
        }
    }
}
