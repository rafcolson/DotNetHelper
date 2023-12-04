using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using static WinFormsLib.Chars;
using static WinFormsLib.Constants;

namespace WinFormsLib
{
    public static class Utils
    {
        private static readonly ToolTip _toolTip = new()
        {
            AutoPopDelay = TOOLTIP_AUTO_POP_DELAY,
            InitialDelay = TOOLTIP_INITIAL_DELAY,
            ReshowDelay = TOOLTIP_RESHOW_DELAY
        };
        private static readonly Label _label = new();

        public static void ShutDown()
        {
            ProcessStartInfo psi = new("shutdown", "/s /t 1")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            Process.Start(psi);
        }

        public enum Language
        {
            [Value("de")]
            [GlobalStringValue("German")]
            German,
            [Value("en")]
            [GlobalStringValue("English")]
            English,
            [Value("es")]
            [GlobalStringValue("Spanish")]
            Spanish,
            [Value("fr")]
            [GlobalStringValue("French")]
            French,
            [Value("it")]
            [GlobalStringValue("Italian")]
            Italian,
            [Value("nl")]
            [GlobalStringValue("Dutch")]
            Dutch,
        }

        [AttributeUsage(AttributeTargets.Field)]
        public class ValueAttribute : Attribute
        {
            public object Value { get; protected set; }
            public ValueAttribute(object value) => Value = value;
        }

        [AttributeUsage(AttributeTargets.Field)]
        public class GlobalStringValueAttribute : Attribute
        {
            public string Value { get; protected set; }
            public GlobalStringValueAttribute(string name) => Value = Globals.ResourceManager.GetString(name) is string s ? s : string.Empty;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private static void Timer_Tick(object? sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Timer timer && timer.Tag is Action action)
            {
                action();
                timer.Tag = null;
                timer.Dispose();
            }
        }

        private static async void Timer_TickAsync(object? sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Timer timer && timer.Tag is Func<Task> function)
            {
                await function();
                timer.Tag = null;
                timer.Dispose();
            }
        }

        public static void Delay(Action action, int interval = 1000)
        {
            System.Windows.Forms.Timer timer = new() { Interval = interval, Enabled = true, Tag = action };
            timer.Tick += Timer_Tick;
        }

        public static void DelayAsync(Func<Task> function, int interval = 1000)
        {
            System.Windows.Forms.Timer timer = new() { Interval = interval, Enabled = true, Tag = function };
            timer.Tick += Timer_TickAsync;
        }

        public static async void OnTaskCompleted(Task task, Action action)
        {
            using Task tc = task.ContinueWith((result) => { action(); });
            await tc;
        }

        public static Task RunCancellableTask(Action action, CancellationTokenSource cancellationTokenSource, Action? onCompletion = null)
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Task task = Task.Run(() => { action(); }, cancellationToken);
            OnTaskCompleted(task, () =>
            {
                Debug.WriteLine($"Disposing task");
                task.Dispose();
                cancellationTokenSource.Dispose();
                onCompletion?.Invoke();
            });
            return task;
        }

        public static bool IsSymbolicLink(string path) => File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);

        public static bool IsValidDirectoryPath(string path) => !string.IsNullOrEmpty(path) && Directory.Exists(path) && !IsSymbolicLink(path);

        public static bool IsValidFilePath(string path) => !string.IsNullOrEmpty(path) && File.Exists(path) && !IsSymbolicLink(path);
        
        public static bool IsValidPath(string path) => !string.IsNullOrEmpty(path) && Directory.Exists(path) || File.Exists(path) && !IsSymbolicLink(path);

        public static bool IsDirectory(string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);

        public static string? GetTargetFile(string path) => new FileInfo(path).LinkTarget;

        public static string? GetTargetDirectory(string path) => new DirectoryInfo(path).LinkTarget;

        public static string GetFileName(string path)
        {
            string n = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(n))
            {
                string ext = GetExtension(n);
                return n != ext ? n : $"Untitled.{ext}";
            }
            return "Untitled";
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path) is string n && !string.IsNullOrEmpty(n) ? n : Globals.FileNameDefault;
        }

        public static string GetPathWithoutExtension(string path)
        {
            string nwe = GetFileNameWithoutExtension(path);
            string pp = GetParentPath(path);
            return string.IsNullOrEmpty(pp) ? nwe : Path.Join(pp, nwe);
        }

        public static string GetExtension(string path) => Path.GetExtension(path) is string e ? e.ToLower() : string.Empty;

        public static string GetDirectoryPath(string path) => Path.GetDirectoryName(path) is string dp ? dp : string.Empty;

        public static string GetDirectoryName(string path) => Path.GetFileName(path) is string fp ? fp : string.Empty;

        public static string GetParentPath(string path)
        {
            return Path.GetDirectoryName(path) is string dp ? dp : Directory.GetDirectoryRoot(path) is string dr ? dr : string.Empty;
        }

        public static string GetValidDirectoryPath(string path = EMPTY_STRING)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (string.IsNullOrEmpty(path))
                {
                    path = @"%USERPROFILE%\OneDrive\Documents"; // Windows 11 hack
                }
            }
            else
            {
                while (!IsValidDirectoryPath(path))
                {
                    switch (Path.GetDirectoryName(path))
                    {
                        case string p: path = p; break;
                        default: return Directory.GetDirectoryRoot(path) is string r ? r : STARTUP_DIRECTORY_DEFAULT;
                    }
                }
            }
            return path;
        }

        public static string GetRelativePath(string parentPath, string fullPath)
        {
            if (parentPath.SplitLast(Path.DirectorySeparatorChar) is string[] sa1)
            {
                parentPath = sa1.First();
            }
            if (fullPath.SplitFirst($"{parentPath}{Path.DirectorySeparatorChar}") is string[] sa2)
            {
                return sa2.Last();
            }
            return fullPath;
        }

        public static string GetPathDuplicate(string path, IEnumerable<string>? excludedPaths = null)
        {
            if (excludedPaths == null)
            {
                string pp = GetParentPath(path);
                return !string.IsNullOrEmpty(pp) && Directory.Exists(pp) ? GetPathDuplicate(path, GetAllPaths(pp)) : path;
            }
            string p = path;
            string pwe = GetPathWithoutExtension(p);
            string ext = GetExtension(p);
            int i = 1;
            while (excludedPaths.Contains(p))
            {
                p = $"{pwe}{SPACE}{LEFT_ROUND_BRACKET}{i}{RIGHT_ROUND_BRACKET}{ext}";
                i += 1;
            }
            return p;
        }

        public static string GetRawPath(string path) => Path.Join(GetParentPath(path), GetRawName(path));

        public static string GetRawName(string path) => $"{GetRawNameWithoutExtension(path)}{GetExtension(path)}";

        public static string GetRawNameWithoutExtension(string path)
        {
            string nwe = GetFileNameWithoutExtension(path);
            if (nwe.SplitLast($"{SPACE}{LEFT_ROUND_BRACKET}") is string[] sa1)
            {
                if (sa1.Last().SplitFirst($"{RIGHT_ROUND_BRACKET}") is string[] sa2)
                {
                    if (sa2.First().IsInteger())
                    {
                        return sa1.First();
                    }
                }
            }
            return nwe;
        }

        public static string[] GetRawPaths(IEnumerable<string> paths)
        {
            HashSet<string> hs = new();
            foreach (string path in paths)
            {
                hs.Add(GetRawPath(path));
            }
            return hs.ToArray();
        }

        public static string[] GetDirectoryPaths(string directoryPath, bool recursive = false)
        {
            string[] paths = Array.Empty<string>();
            try
            {
                List<string> l = Directory.EnumerateDirectories(directoryPath, "*.*", SearchOption.TopDirectoryOnly).ToList();
                foreach (string dp in l.ToArray())
                {
                    if (IsSymbolicLink(dp))
                    {
                        if (GetTargetDirectory(dp) is string tp)
                        {
                            l.Replace(dp, tp);
                        }
                    }
                }
                paths = l.ToArray();
                if (recursive)
                {
                    foreach (string dp in paths)
                    {
                        l.AddRange(GetDirectoryPaths(dp));
                    }
                    paths = l.ToArray();
                }
                else
                {
                    Array.Sort(paths);
                }
            }
            catch { }
            return paths;
        }

        public static string[] GetFilePaths(string directoryPath, bool recursive = false)
        {
            try
            {
                SearchOption so = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] paths = Directory.EnumerateFiles(directoryPath, "*.*", so).ToArray();
                if (!recursive)
                {
                    Array.Sort(paths);
                }
                return paths;
            }
            catch { return Array.Empty<string>(); }
        }

        public static string[] GetAllPaths(string directoryPath, bool recursive = false)
        {
            string[] directoryPaths = GetDirectoryPaths(directoryPath, recursive);
            string[] filePaths = GetFilePaths(directoryPath, recursive);
            return directoryPaths.Concat(filePaths).ToArray();
        }

        public static string? GetSpecialFolder(string directoryPath)
        {
            foreach (string sf in GetSpecialfolders())
            {
                if (string.Equals(directoryPath, sf, StringComparison.OrdinalIgnoreCase))
                {
                    return sf;
                }
            }
            return null;
        }

        public static string[] GetSpecialfolders()
        {
            List<string> l = new();
            foreach (Environment.SpecialFolder specialFolder in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                l.Add(Environment.GetFolderPath(specialFolder));
            }
            return l.ToArray();
        }

        public static bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public static bool DeleteDirectory(string path)
        {
            try
            {
                Directory.Delete(path, true);
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public static bool MoveFile(string sourcePath, string destinationPath)
        {
            if (sourcePath == destinationPath)
            {
                return false;
            }
            string pp = GetParentPath(destinationPath);
            bool ppExists = Directory.Exists(pp);
            if (!ppExists)
            {
                Directory.CreateDirectory(pp);
            }
            try
            {
                File.Move(sourcePath, destinationPath);
                return true;
            }
            catch (Exception)
            {
                if (!ppExists)
                {
                    DeleteDirectory(pp);
                }
                return false;
            }
        }

        public static bool MoveDirectory(string sourcePath, string destinationPath)
        {
            if (sourcePath == destinationPath)
            {
                return false;
            }
            bool dpExists = Directory.Exists(destinationPath);
            if (!dpExists)
            {
                Directory.CreateDirectory(destinationPath);
            }
            try
            {
                Directory.Move(sourcePath, destinationPath);
                return true;
            }
            catch (Exception)
            {
                if (!dpExists)
                {
                    DeleteDirectory(destinationPath);
                }
                return false;
            }
        }

        public static bool CopyFile(string sourcePath, string destinationPath)
        {
            if (sourcePath == destinationPath)
            {
                return false;
            }
            string pp = GetParentPath(destinationPath);
            bool ppExists = Directory.Exists(pp);
            if (!ppExists)
            {
                Directory.CreateDirectory(pp);
            }
            try
            {
                File.Copy(sourcePath, destinationPath);
                return true;
            }
            catch (Exception)
            {
                if (!ppExists)
                {
                    DeleteDirectory(pp);
                }
                return false;
            }
        }

        public static bool CopyDirectory(string sourcePath, string destinationPath)
        {
            if (sourcePath == destinationPath)
            {
                return false;
            }
            bool dpExists = Directory.Exists(destinationPath);
            if (!dpExists)
            {
                Directory.CreateDirectory(destinationPath);
            }
            try
            {
                foreach (string sp in GetAllPaths(sourcePath, true))
                {
                    string rp = GetRelativePath(sourcePath, sp);
                    string dp = Path.Join(destinationPath, rp);
                    CopyFile(sp, dp);
                }
                return true;
            }
            catch (Exception)
            {
                if (!dpExists)
                {
                    DeleteDirectory(destinationPath);
                }
                return false;
            }
        }

        public static string RenameFile(string path, string name, string parentPath = EMPTY_STRING)
        {
            if (string.IsNullOrEmpty(parentPath))
            {
                parentPath = GetParentPath(path);
            }
            else if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }
            string rawPath = Path.Join(parentPath, $"{name}{GetExtension(path)}");
            if (rawPath != path)
            {
                string newPath = File.Exists(rawPath) ? GetPathDuplicate(rawPath) : rawPath;
                try
                {
                    File.Move(path, newPath);
                    return newPath;
                }
                catch (Exception) { }
            }
            return path;
        }

        public static byte[] Zipped(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            using MemoryStream msi = new(bytes);
            using MemoryStream mso = new();
            using (GZipStream gs = new(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }
            msi.Close();
            return mso.ToArray();
        }

        public static string Unzipped(byte[] bytes)
        {
            using MemoryStream msi = new(bytes);
            using MemoryStream mso = new();
            using (GZipStream gs = new(msi, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }
            msi.Close();
            return Encoding.UTF8.GetString(mso.ToArray());
        }

        public static Icon GetIcon(string path)
        {
            Bitmap bitmap = new($"@{path}");
            IntPtr Hicon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(Hicon);
            DestroyIcon(icon.Handle);
            return icon;
        }

        public static void Execute(string path)
        {
            if (File.Exists(path))
            {
                if (GetExtension(path) == ".exe")
                {
                    //ProcessStartInfo psi = new()
                    //{
                    //    Arguments = super.FullName,
                    //    UseShellExecute = true,
                    //    WorkingDirectory = super.DirectoryName,
                    //    FileName = super.FullName,
                    //    Verb = "OPEN"
                    //};
                    try { _ = Process.Start($@"{path}"); } catch { }
                }
                else
                {
                    throw new ArgumentException($"{path} is not an executable.");
                }
            }
            else
            {
                throw new IOException($"{path} does not exist.");
            }
        }

        public static void MatchSizes(Control.ControlCollection controlCollection)
        {
            IEnumerable<Control> ie = controlCollection.Cast<Control>();
            int w = new List<int>(ie.Select(x => x.Width)).Max();
            int h = new List<int>(ie.Select(x => x.Height)).Max();
            foreach (Control c in controlCollection)
            {
                c.MinimumSize = new(w, h);
            }
        }

        public static Size CalculateSize
        (
            string text,
            Font? font = null,
            Size? dimensions = null,
            Padding? padding = null,
            TextFormatFlags format = TextFormatFlags.WordBreak
        )
        {
            font ??= FONT_DEFAULT;
            padding ??= new();
            Padding p = PADDING_DEFAULT + padding.Value;
            Size proposedSize = dimensions ?? new(16, 9);
            Size size = TextRenderer.MeasureText(text, font, proposedSize, format);
            double sqr = Math.Sqrt((size.Height + p.Vertical) * (size.Width + p.Horizontal));
            double r = (double)proposedSize.Width / proposedSize.Height;
            int h = (int)Math.Ceiling(sqr / r + Math.Sqrt(p.Vertical));
            int w = (int)Math.Ceiling(sqr * r + Math.Sqrt(p.Horizontal));
            return new(w, h);
        }

        public static int GetMaxWidth(object[] objects, Font font, int marginHorizontal = 0, int minWidth = 0)
        {
            int marginOfError = 2;
            List<int> l = new() { marginHorizontal + marginOfError, minWidth};
            _label.Font = font;
            foreach (object o in objects)
            {
                _label.Text = $"{o}";
                l.Add(_label.PreferredWidth + marginHorizontal + marginOfError);
            }
            return l.Max();
        }

        public static int GetMaxHeight(object[] objects, Font font, int marginVertical = 0, int minHeight = 0)
        {
            int marginOfError = 2;
            List<int> l = new() { marginVertical + marginOfError, minHeight };
            _label.Font = font;
            foreach (object o in objects)
            {
                _label.Text = $"{o}";
                l.Add(_label.PreferredHeight + marginVertical + marginOfError);
            }
            return l.Max();
        }

        public static Size GetMaxSize(object[] objects, Font font, Padding margin, Size? minSize = null)
        {
            minSize ??= new();
            int mw = minSize.Value.Width;
            int mh = minSize.Value.Height;
            return new(GetMaxWidth(objects, font, margin.Horizontal, mw), GetMaxHeight(objects, font, margin.Vertical, mh));
        }

        public static Size GetTableLayout(int numItems, bool landscape = true)
        {
            int u = (int)Math.Sqrt(numItems);
            int r = numItems - (int)Math.Pow(u, 2);
            int w = landscape ? u + r : u;
            int h = landscape ? u : u + r;
            return new(w, h);
        }

        public static string[] GetUrls(string text)
        {
            List<string> l = new();
            Regex regex = new(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);
            l.AddRange(regex.Matches(text).Select(match => match.Value.TrimEnd(PERIOD)));
            return l.ToArray();
        }

        public static LinkLabel.Link[] GetLinkLabelLinks(string text, Dictionary<string, string>? textLinkPairs = null)
        {
            List<LinkLabel.Link> l = new();
            textLinkPairs ??= GetUrls(text).ToDictionary(x => x);
            l.AddRange(textLinkPairs.Select(kvp => new LinkLabel.Link(text.IndexOf(kvp.Key), kvp.Key.Length) { LinkData = kvp.Value }));
            return l.ToArray();
        }

        public static string GetValidTwoLetterISOLanguageName(Dictionary<string, string[]> globals, CultureInfo? cultureInfo)
        {
            if (cultureInfo == null || cultureInfo == CULTURE_INFO_DEFAULT)
            {
                cultureInfo = CultureInfo.CurrentCulture;
            }
            string n = cultureInfo.TwoLetterISOLanguageName;
            string[] supportedLanguages = globals.Keys.ToArray();
            if (!supportedLanguages.Contains(n))
            {
                n = supportedLanguages[1];
            }
            return n;
        }

        public static string GetTempResource(string name, byte[] resource)
        {
            string path = Path.Join(Path.GetTempPath(), name);
            if (!File.Exists(path))
            {
                using FileStream fs = new(path, FileMode.CreateNew);
                fs.Write(resource, 0, resource.Length);
            }
            return path;
        }

        public static void AddToolTip(this Control control, string caption) => _toolTip.SetToolTip(control, caption);

        public static Image? GetThumbnailImage(Bitmap? bmp) => bmp?.GetThumbnailImage(bmp.Width, bmp.Height, null, IntPtr.Zero);

        public static void Dispose()
        {
            Debug.WriteLine($"Disposing 'Utils'...");

            _toolTip.Active = false;
            _toolTip.RemoveAll();
            _toolTip.Dispose();
            _label.Dispose();
        }
    }
}
