using System.ComponentModel;

namespace WinFormsLib
{
    /// <summary>Provides traversal helpers for Windows Forms components.</summary>
    public static class ComponentExtensions
    {
        /// <summary>Enumerates child controls or tool strip items and their supported descendants.</summary>
        /// <param name="super">The root component, which is excluded from the result.</param>
        /// <returns>A sequence that yields the immediate children before traversing their descendants.</returns>
        /// <remarks>The traversal follows <see cref="ToolStrip.Items"/> for tool strips and <see cref="Control.Controls"/> for other controls.</remarks>
        public static IEnumerable<Component> GetAllComponents(this Component super)
        {
            IEnumerable<Component> components = super is ToolStrip ts ? ts.Items.Cast<Component>() : super is Control c ? c.Controls.Cast<Component>() : [];
            return components.Concat(components.SelectMany(x => x.GetAllComponents()));
        }
    }
}
