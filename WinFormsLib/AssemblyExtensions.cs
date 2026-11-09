using System.Reflection;

namespace WinFormsLib
{
    /// <summary>Provides helpers for reading assembly identity and metadata.</summary>
    public static class AssemblyExtensions
    {
        /// <summary>Gets the first assembly attribute of the requested type.</summary>
        /// <typeparam name="T">The attribute type to look up.</typeparam>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The first matching attribute, or the default value of <typeparamref name="T"/> if none is present.</returns>
        public static T? GetAssemblyAttribute<T>(this Assembly super)
        {
            object[] attributes = super.GetCustomAttributes(typeof(T), false);
            return attributes.Length != 0 ? (T)attributes[0] : default;
        }

        /// <summary>Gets the assembly's simple name.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetSimpleName(this Assembly super) => super.GetName().Name is string s ? s : string.Empty;

        /// <summary>Gets the full assembly identity.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetFullName(this Assembly super) => super.GetName().FullName is string s ? s : string.Empty;

        /// <summary>Gets the title from the assembly title attribute.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetTitle(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyTitleAttribute>() is AssemblyTitleAttribute attr ? attr.Title : string.Empty;
        }

        /// <summary>Gets the product name from the assembly product attribute.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetProduct(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyProductAttribute>() is AssemblyProductAttribute attr ? attr.Product : string.Empty;
        }

        /// <summary>Gets the alias from the assembly default alias attribute.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetDefaultAlias(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyDefaultAliasAttribute>() is AssemblyDefaultAliasAttribute attr ? attr.DefaultAlias : string.Empty;
        }

        /// <summary>Gets the file version, as returned by <see cref="GetFileVersion"/>.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetVersion(this Assembly super) => GetFileVersion(super);

        /// <summary>Gets the version from the assembly file version attribute.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetFileVersion(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyFileVersionAttribute>() is AssemblyFileVersionAttribute attr ? attr.Version : string.Empty;
        }

        /// <summary>Gets the version from the assembly identity.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetAssemblyVersion(this Assembly super) => super.GetName().Version?.ToString() ?? string.Empty;

        /// <summary>Gets the version from the assembly informational version attribute.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetInformationalVersion(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyInformationalVersionAttribute>() is AssemblyInformationalVersionAttribute attr ? attr.InformationalVersion : string.Empty;
        }

        /// <summary>Gets the copyright text from the assembly copyright attribute.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetCopyright(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyCopyrightAttribute>() is AssemblyCopyrightAttribute attr ? attr.Copyright : string.Empty;
        }

        /// <summary>Gets the description from the assembly description attribute.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetDescription(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyDescriptionAttribute>() is AssemblyDescriptionAttribute attr ? attr.Description : string.Empty;
        }

        /// <summary>Gets the company name from the assembly company attribute.</summary>
        /// <param name="super">The assembly to inspect.</param>
        /// <returns>The requested metadata, or an empty string if it is unavailable.</returns>
        public static string GetCompany(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyCompanyAttribute>() is AssemblyCompanyAttribute attr ? attr.Company : string.Empty;
        }

    }
}
