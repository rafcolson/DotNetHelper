using System.ComponentModel;

namespace WinFormsLib
{
    public static class ComponentExtensions
    {
        public static IEnumerable<Component> GetAllComponents(this Component super)
        {
            IEnumerable<Component> components = super is ToolStrip ts ? ts.Items.Cast<Component>() : super is Control c ? c.Controls.Cast<Component>() : [];
            return components.Concat(components.SelectMany(x => x.GetAllComponents()));
        }
    }
}
