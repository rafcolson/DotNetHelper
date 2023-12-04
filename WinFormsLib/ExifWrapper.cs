using System.Diagnostics;

using static WinFormsLib.Utils;

namespace WinFormsLib
{
    public static class ExifWrapper
    {
        public static Process Tool { get; private set; }
        public static string ToolFmtPath => GetTempResource("EFM_exiftool_json.fmt", Properties.Resources.exiftool_json);
        public static string ToolExePath => GetTempResource("EFM_exiftool.exe", Properties.Resources.exiftool);

        static ExifWrapper() => Tool = new() { StartInfo = new ProcessStartInfo(ToolExePath) { RedirectStandardOutput = true, CreateNoWindow = true } };

        public static void Dispose()
        {
            Debug.WriteLine($"Disposing 'ExifWrapper'...");

            Tool.Dispose();
        }
    }
}
