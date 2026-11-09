using System.Diagnostics;

using static WinFormsLib.Utils;

namespace WinFormsLib
{
    /// <summary>Exposes the bundled ExifTool executable and creates processes for invoking it.</summary>
    public static class ExifWrapper
    {
        private static readonly object ResourceLock = new();

        /// <summary>Gets the shared ExifTool process with standard output redirected.</summary>
        /// <remarks>Use the process for one operation at a time. Callers configure its arguments and read its output.</remarks>
        public static Process Tool { get; private set; }

        /// <summary>Gets the path to the ExifTool JSON output template, extracting it to the system temporary directory if absent.</summary>
        public static string ToolFmtPath
        {
            get
            {
                lock (ResourceLock)
                {
                    return GetTempResource("EFM_exiftool_json.fmt", Properties.Resources.exiftool_json);
                }
            }
        }

        /// <summary>Gets the path to the bundled ExifTool executable, extracting it to the system temporary directory if absent.</summary>
        public static string ToolExePath
        {
            get
            {
                lock (ResourceLock)
                {
                    return GetTempResource("EFM_exiftool.exe", Properties.Resources.exiftool);
                }
            }
        }

        static ExifWrapper() => Tool = CreateToolProcess();

        /// <summary>Creates an unstarted ExifTool process with standard output redirected.</summary>
        /// <returns>A separate process component that the caller must dispose.</returns>
        public static Process CreateToolProcess() => new()
        {
            StartInfo = new ProcessStartInfo(ToolExePath) { RedirectStandardOutput = true, CreateNoWindow = true }
        };

        /// <summary>Releases the shared process component. Call only after all ExifTool operations have finished.</summary>
        public static void Dispose()
        {
            Debug.WriteLine($"Disposing 'ExifWrapper'...");

            Tool.Dispose();
        }
    }
}
