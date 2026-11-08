using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WinFormsLib
{
    public static partial class ControlExtensions
    {
        private const int WM_SETREDRAW = 11;

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
        private static partial nint SendMessage(nint hWnd, int wMsg, nint wParam, nint lParam);

        public static void SuspendDrawing(this Control super) => _ = SendMessage(super.Handle, WM_SETREDRAW, 0, 0);

        public static void ResumeDrawing(this Control super)
        {
            _ = SendMessage(super.Handle, WM_SETREDRAW, 1, 0);
            super.Refresh();
        }

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
