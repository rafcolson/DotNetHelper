namespace WinFormsLib
{
    public static class ComboBoxExtensions
    {
        public static void UpdateDropDownList(this ComboBox super)
        {
            DockStyle ds = super.Dock;
            super.Dock = DockStyle.None;
            super.DropDownWidth = Utils.GetMaxWidth(super.Items.Cast<object>().ToArray(), super.Font);
            super.Dock = ds;
        }
    }
}
