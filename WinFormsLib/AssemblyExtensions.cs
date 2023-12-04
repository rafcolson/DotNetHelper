using System.Reflection;

namespace WinFormsLib
{
    public static class AssemblyExtensions
    {
        public static T? GetAssemblyAttribute<T>(this Assembly super)
        {
            object[] attributes = super.GetCustomAttributes(typeof(T), false);
            return attributes.Any() ? (T)attributes[0] : default;
        }

        public static string GetSimpleName(this Assembly super) => super.GetName().Name is string s ? s : string.Empty;

        public static string GetFullName(this Assembly super) => super.GetName().FullName is string s ? s : string.Empty;

        public static string GetTitle(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyTitleAttribute>() is AssemblyTitleAttribute attr ? attr.Title : string.Empty;
        }

        public static string GetProduct(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyProductAttribute>() is AssemblyProductAttribute attr ? attr.Product : string.Empty;
        }

        public static string GetDefaultAlias(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyDefaultAliasAttribute>() is AssemblyDefaultAliasAttribute attr ? attr.DefaultAlias : string.Empty;
        }

        public static string GetVersion(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyVersionAttribute>() is AssemblyVersionAttribute attr ? attr.Version : string.Empty;
        }

        public static string GetFileVersion(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyFileVersionAttribute>() is AssemblyFileVersionAttribute attr ? attr.Version : string.Empty;
        }

        public static string GetInformationalVersion(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyInformationalVersionAttribute>() is AssemblyInformationalVersionAttribute attr ? attr.InformationalVersion : string.Empty;
        }

        public static string GetCopyright(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyCopyrightAttribute>() is AssemblyCopyrightAttribute attr ? attr.Copyright : string.Empty;
        }

        public static string GetDescription(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyDescriptionAttribute>() is AssemblyDescriptionAttribute attr ? attr.Description : string.Empty;
        }

        public static string GetCompany(this Assembly super)
        {
            return super.GetAssemblyAttribute<AssemblyCompanyAttribute>() is AssemblyCompanyAttribute attr ? attr.Company : string.Empty;
        }


    }
}
