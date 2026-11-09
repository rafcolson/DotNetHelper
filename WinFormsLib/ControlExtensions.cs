using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WinFormsLib
{
    /// <summary>Provides helpers for control redraw, traversal, and cleanup.</summary>
    public static partial class ControlExtensions
    {
        private const int WM_SETREDRAW = 11;

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
        private static partial nint SendMessage(nint hWnd, int wMsg, nint wParam, nint lParam);

        /// <summary>Disables redraw for the control's window.</summary>
        /// <param name="super">The control whose redraw is suspended.</param>
        /// <remarks>Call <see cref="ResumeDrawing"/> when the updates are complete.</remarks>
        public static void SuspendDrawing(this Control super) => _ = SendMessage(super.Handle, WM_SETREDRAW, 0, 0);

        /// <summary>Enables redraw and refreshes the control immediately.</summary>
        /// <param name="super">The control to redraw.</param>
        public static void ResumeDrawing(this Control super)
        {
            _ = SendMessage(super.Handle, WM_SETREDRAW, 1, 0);
            super.Refresh();
        }

        /// <summary>Returns child controls of the requested type, with descendants appearing before their parents.</summary>
        /// <typeparam name="T">The control type to match.</typeparam>
        /// <param name="super">The root control, which is excluded from the result.</param>
        /// <param name="recursive">Whether to include descendants beyond the immediate children.</param>
        public static T[] GetControls<T>(this Control super, bool recursive = true) where T : Control
        {
            List<T> l = [];
            foreach (Control c in super.Controls)
            {
                if (recursive)
                {
                    l.AddRange(c.GetControls<T>(true));
                }
                if (c is T t)
                {
                    l.Add(t);
                }
            }
            return [.. l];
        }

        private static Control? GetFocusedControl(Control super)
        {
            if (super.Focused)
            {
                return super;
            }
            foreach (Control ctrl in super.Controls)
            {
                if (GetFocusedControl(ctrl) is Control child)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>Uses reflection to clear the control's event handler list, if found.</summary>
        /// <typeparam name="T">The control type used for the lookup.</typeparam>
        /// <param name="super">The control whose event handler list is cleared.</param>
        public static void ClearEventHandlers<T>(this T super) where T : Control
        {
            PropertyInfo? propInfo = typeof(T).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            using EventHandlerList? list = (EventHandlerList?)propInfo?.GetValue(super, null);
            list?.Dispose();
        }

        /// <summary>Removes and disposes descendant controls and clears event handlers on the root control.</summary>
        /// <param name="super">The root control to clear.</param>
        /// <remarks>Event handlers on descendants are cleared before those controls are disposed.</remarks>
        public static void Clear(this Control super)
        {
            foreach (Control c in super.GetControls<Control>(true))
            {
                c.ClearEventHandlers();
                c.Controls.Clear();
                c.Dispose();
            }
            super.Controls.Clear();
            super.ClearEventHandlers();
        }
    }
}
