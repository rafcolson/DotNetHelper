using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WinFormsLib
{
    public static class ControlExtensions
    {
        private const int WM_SETREDRAW = 11;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        public static void SuspendDrawing(this Control super) => _ = SendMessage(super.Handle, WM_SETREDRAW, false, 0);

        public static void ResumeDrawing(this Control super)
        {
            _ = SendMessage(super.Handle, WM_SETREDRAW, true, 0);
            super.Refresh();
        }

        public static T[] GetControls<T>(this Control super, bool recursive = true) where T : Control
        {
            List<T> l = new();
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
            return l.ToArray();
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

        public static void ClearEventHandlers<T>(this T super) where T : Control
        {
            PropertyInfo? propInfo = typeof(T).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            using EventHandlerList? list = (EventHandlerList?)propInfo?.GetValue(super, null);
            list?.Dispose();
        }

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
