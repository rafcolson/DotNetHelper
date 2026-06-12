using static WinFormsLib.Utils;

namespace WinFormsLib
{
    public static class ToolStripMenuItems
    {
        [Flags]
        public enum EditMenuItem
        {
            None = 0,
            [Value("^(Z)")]
            [GlobalStringValue("Undo")]
            Undo = 1 << 0,
            [Value("^(X)")]
            [GlobalStringValue("Cut")]
            Cut = 1 << 1,
            [Value("^(C)")]
            [GlobalStringValue("Copy")]
            Copy = 1 << 2,
            [Value("^(V)")]
            [GlobalStringValue("Paste")]
            Paste = 1 << 3,
            [Value("^(a)")]
            [GlobalStringValue("SelectAll")]
            SelectAll = 1 << 4
        }

        public enum EditMenuItems
        {
            None = EditMenuItem.None,
            CopySelectAll = EditMenuItem.Copy | EditMenuItem.SelectAll,
            CutCopyPasteSelectAll = EditMenuItem.Cut | EditMenuItem.Copy | EditMenuItem.Paste | EditMenuItem.SelectAll,
            UndoCutCopyPasteSelectAll = EditMenuItem.Undo | EditMenuItem.Cut | EditMenuItem.Copy | EditMenuItem.Paste | EditMenuItem.SelectAll
        }

        private static void ToolStripEditMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi && tsmi.Tag is EditMenuItem emi)
            {
                string? keys = emi.GetValue<string>();
                if (!string.IsNullOrEmpty(keys))
                {
                    SendKeys.Send(keys);
                }
            }
        }

        public static ToolStripMenuItem? GetToolStripMenuItem(string text, EventHandler eventHandler, object? tag = null, Bitmap? bitmap = null)
        {
            ToolStripMenuItem tsmi = new()
            {
                Name = $"ToolStripMenuItem{text}",
                AutoSize = true,
                Text = text,
                Tag = tag,
                Image = bitmap
            };
            tsmi.Click += eventHandler;
            tsmi.Disposed += (sender, e) => tsmi.Click -= eventHandler;
            return tsmi;
        }

        public static ToolStripMenuItem? GetToolStripMenuItem(EditMenuItem editMenuItem)
        {
            string s = editMenuItem.GetGlobalStringValue();
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            string? n = Enum.GetName(editMenuItem);
            Bitmap? bm = !string.IsNullOrEmpty(n) ? (Bitmap?)Properties.Resources.ResourceManager.GetObject(n) : null;
            return GetToolStripMenuItem(s, ToolStripEditMenuItem_Click, editMenuItem, bm);
        }

        public static ToolStripMenuItem[] GetToolStripMenuItems(EditMenuItems editMenuItems = EditMenuItems.UndoCutCopyPasteSelectAll)
        {
            List<ToolStripMenuItem> toolStripMenuItems = [];
            foreach (EditMenuItem menuItem in editMenuItems.ToEnum<EditMenuItem>().GetContainingFlags())
            {
                if (GetToolStripMenuItem(menuItem) is ToolStripMenuItem tsmi)
                {
                    toolStripMenuItems.Add(tsmi);
                }
            }
            return [.. toolStripMenuItems];
        }
    }
}
