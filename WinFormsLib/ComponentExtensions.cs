using System.ComponentModel;

namespace WinFormsLib
{
    public static class ComponentExtensions
    {
        public static IEnumerable<Component> GetAllComponents(this Component super)
        {
            IEnumerable<Component> components;
            if (super is ToolStrip ts) components = ts.Items.Cast<Component>();
            else if (super is Control c) components = c.Controls.Cast<Component>();
            else components = Enumerable.Empty<Component>();
            return components.Concat(components.SelectMany(x => x.GetAllComponents()));
        }
    }
}
