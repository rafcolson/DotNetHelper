using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WinFormsLib
{
    /// <summary>Identifies installations using a registry key managed by the installer.</summary>
    public static partial class Installation
    {
        [LibraryImport("advapi32.dll", EntryPoint = "RegQueryInfoKeyW")]
        private static partial int RegQueryInfoKey(
            SafeRegistryHandle key,
            IntPtr className,
            IntPtr classNameLength,
            IntPtr reserved,
            IntPtr subKeyCount,
            IntPtr maximumSubKeyLength,
            IntPtr maximumClassLength,
            IntPtr valueCount,
            IntPtr maximumValueNameLength,
            IntPtr maximumValueLength,
            IntPtr securityDescriptorLength,
            out long lastWriteTime);

        /// <summary>Returns the installation key's last-write time, or zero if the key does not exist.</summary>
        /// <param name="companyName">The company's registry key name.</param>
        /// <param name="productName">The product's registry key name.</param>
        /// <remarks>
        /// The installer must create HKLM\SOFTWARE\companyName\productName\Installation in the
        /// 64-bit registry view and remove that key at uninstall. The application only reads it.
        /// </remarks>
        public static long GetStamp(string companyName, string productName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
            ArgumentException.ThrowIfNullOrWhiteSpace(productName);
            string registryPath = $@"SOFTWARE\{companyName}\{productName}\Installation";
            using RegistryKey machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey? installation = machine.OpenSubKey(registryPath);
            if (installation == null)
            {
                return 0;
            }
            int result = RegQueryInfoKey(installation.Handle,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                out long stamp);
            return result != 0 ? throw new Win32Exception(result) : stamp;
        }
    }
}
