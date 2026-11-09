using static WinFormsLib.Utils;

namespace WinFormsLib
{
    /// <summary>Creates Windows Forms menu items, including standard editing commands.</summary>
    public static class ToolStripMenuItems
    {
        /// <summary>Identifies individual editing commands that can be combined as flags.</summary>
        [Flags]
        public enum EditMenuItem
        {
            /// <summary>Specifies no editing command.</summary>
            None = 0,
            /// <summary>Undoes the last edit by sending Ctrl+Z.</summary>
            [Value("^(Z)")]
            [GlobalStringValue("Undo")]
            Undo = 1 << 0,
            /// <summary>Cuts the selection by sending Ctrl+X.</summary>
            [Value("^(X)")]
            [GlobalStringValue("Cut")]
            Cut = 1 << 1,
            /// <summary>Copies the selection by sending Ctrl+C.</summary>
            [Value("^(C)")]
            [GlobalStringValue("Copy")]
            Copy = 1 << 2,
            /// <summary>Pastes clipboard contents by sending Ctrl+V.</summary>
            [Value("^(V)")]
            [GlobalStringValue("Paste")]
            Paste = 1 << 3,
            /// <summary>Selects all content by sending Ctrl+A.</summary>
            [Value("^(a)")]
            [GlobalStringValue("SelectAll")]
            SelectAll = 1 << 4
        }

        /// <summary>Defines preset groups of editing commands.</summary>
        public enum EditMenuItems
        {
            /// <summary>Specifies an empty command group.</summary>
            None = EditMenuItem.None,
            /// <summary>Includes Copy and Select All.</summary>
            CopySelectAll = EditMenuItem.Copy | EditMenuItem.SelectAll,
            /// <summary>Includes Cut, Copy, Paste, and Select All.</summary>
            CutCopyPasteSelectAll = EditMenuItem.Cut | EditMenuItem.Copy | EditMenuItem.Paste | EditMenuItem.SelectAll,
            /// <summary>Includes Undo, Cut, Copy, Paste, and Select All.</summary>
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

        /// <summary>Creates a menu item with the supplied display text and click handler.</summary>
        /// <param name="text">The display text.</param>
        /// <param name="eventHandler">The handler invoked when the item is clicked.</param>
        /// <param name="tag">Optional data to attach to the item.</param>
        /// <param name="bitmap">The optional item image.</param>
        /// <returns>The created menu item.</returns>
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

        /// <summary>Creates a localized menu item for an editing command.</summary>
        /// <param name="editMenuItem">The individual editing command.</param>
        /// <returns>The menu item, or null if no localized label is available for the command.</returns>
        /// <remarks>The command sends its shortcut to the window that has focus.</remarks>
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

        /// <summary>Creates localized menu items for the individual commands in a group.</summary>
        /// <param name="editMenuItems">The group of commands to include.</param>
        /// <returns>The created menu items, excluding commands without a localized label.</returns>
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
