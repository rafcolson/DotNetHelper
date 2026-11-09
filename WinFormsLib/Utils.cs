using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static WinFormsLib.Chars;
using static WinFormsLib.Constants;

namespace WinFormsLib
{
    /// <summary>Provides Windows Forms, text search, file system, and application helpers.</summary>
    public static partial class Utils
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int WM_CANCELMODE = 0x001F;
        private const int WM_THEMECHANGED = 0x031A;

        [LibraryImport("dwmapi.dll")]
        private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);

        [LibraryImport("uxtheme.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SetWindowTheme(IntPtr hwnd, string? subAppName, string? subIdList);

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
        private static partial IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);

        [ComImport]
        [Guid("56FDF342-FD6D-11D0-958A-006097C9A090")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList
        {
            void HrInit();
            void AddTab(IntPtr windowHandle);
            void DeleteTab(IntPtr windowHandle);
        }

        /// <summary>Adds an existing window to the Windows taskbar.</summary>
        /// <param name="windowHandle">The native handle of the window to add.</param>
        /// <remarks>
        /// Call on the window's UI thread. Remove the entry with
        /// <see cref="RemoveWindowFromTaskbar"/> before destroying the window handle.
        /// </remarks>
        /// <returns>Whether the taskbar entry was added successfully.</returns>
        public static bool AddWindowToTaskbar(IntPtr windowHandle) => UpdateTaskbarWindow(windowHandle, add: true);

        /// <summary>Removes a window's entry from the Windows taskbar.</summary>
        /// <param name="windowHandle">The native handle of the window to remove.</param>
        /// <remarks>Call on the window's UI thread while the window handle is still valid.</remarks>
        /// <returns>Whether the taskbar entry was removed successfully.</returns>
        public static bool RemoveWindowFromTaskbar(IntPtr windowHandle) => UpdateTaskbarWindow(windowHandle, add: false);

        private static bool UpdateTaskbarWindow(IntPtr windowHandle, bool add)
        {
            object? instance = null;
            try
            {
                Type taskbarType = Type.GetTypeFromCLSID(new Guid("56FDF344-FD6D-11D0-958A-006097C9A090"))!;
                instance = Activator.CreateInstance(taskbarType)!;
                ITaskbarList taskbar = (ITaskbarList)instance;
                taskbar.HrInit();
                if (add)
                {
                    taskbar.AddTab(windowHandle);
                }
                else
                {
                    taskbar.DeleteTab(windowHandle);
                }
                return true;
            }
            catch (COMException)
            {
                return false;
            }
            finally
            {
                if (instance != null)
                {
                    _ = Marshal.ReleaseComObject(instance);
                }
            }
        }

        private static readonly ToolTip _toolTip = new()
        {
            AutoPopDelay = TOOLTIP_AUTO_POP_DELAY,
            InitialDelay = TOOLTIP_INITIAL_DELAY,
            ReshowDelay = TOOLTIP_RESHOW_DELAY
        };
        private static readonly ToolTip _toolStripToolTip = new()
        {
            AutoPopDelay = TOOLTIP_AUTO_POP_DELAY,
            InitialDelay = TOOLTIP_INITIAL_DELAY,
            ReshowDelay = TOOLTIP_RESHOW_DELAY
        };
        private static readonly Label _label = new();
        private static bool _toolTipOwnerDrawConfigured;
        private static readonly Dictionary<ToolStripItem, string> _toolStripToolTips = [];
        private static readonly System.Windows.Forms.Timer _toolStripToolTipTimer = new()
        {
            Interval = TOOLTIP_INITIAL_DELAY
        };
        private static ToolStripItem? _pendingToolStripToolTip;
        private static bool _toolStripToolTipTimerConfigured;
        private static readonly Dictionary<TabControl, int?> _tabToolTips = [];
        private static readonly System.Windows.Forms.Timer _tabToolTipTimer = new()
        {
            Interval = TOOLTIP_INITIAL_DELAY
        };
        private static int _tabToolTipInitialDelay = TOOLTIP_INITIAL_DELAY;
        private static TabControl? _pendingTabToolTip;
        private static int _pendingTabIndex = -1;
        private static bool _tabToolTipTimerConfigured;

        /// <summary>Specifies how search text is matched.</summary>
        public enum WordSearchOptions
        {
            /// <summary>Requires a match for every search term.</summary>
            AllWords,
            /// <summary>Returns matches for any search term.</summary>
            AnyWord,
            /// <summary>Treats the entire search phrase as a single term.</summary>
            ExactPhrase
        }

        /// <summary>Requests the selected scroll bar theme for a control with an existing handle.</summary>
        /// <param name="control">The control to update.</param>
        /// <param name="dark">Whether to request dark styling.</param>
        public static void SetScrollBarTheme(Control control, bool dark)
        {
            if (!control.IsHandleCreated)
            {
                return;
            }

            _ = SetWindowTheme(control.Handle, dark ? "DarkMode_Explorer" : null, null);
            _ = SendMessage(control.Handle, WM_THEMECHANGED, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>Cancels the current native interaction mode for a control with an existing handle.</summary>
        /// <param name="control">The control to update.</param>
        public static void CancelMode(Control control)
        {
            if (control.IsHandleCreated)
            {
                _ = SendMessage(control.Handle, WM_CANCELMODE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        /// <summary>Requests the selected title bar theme for a form on supported Windows versions.</summary>
        /// <param name="form">The form whose title bar is updated.</param>
        /// <param name="dark">Whether to request dark styling.</param>
        public static void SetTitleBarTheme(Form form, bool dark)
        {
            if (!form.IsHandleCreated || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
            {
                return;
            }

            int value = dark ? 1 : 0;
            int attribute = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18985)
                ? DWMWA_USE_IMMERSIVE_DARK_MODE
                : DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            _ = DwmSetWindowAttribute(form.Handle, attribute, ref value, sizeof(int));
        }

        /// <summary>The shared set of punctuation and spacing characters used for text processing.</summary>
        public static readonly HashSet<char> CommonPunctuationMarks = [Chars.SPACE, Chars.PERIOD, Chars.COMMA, Chars.QUESTION_MARK, Chars.EXCLAMATION_MARK, Chars.COLON, Chars.SEMICOLON, Chars.HYPHEN, Chars.SINGLE_QUOTE, Chars.DOUBLE_QUOTE, Chars.LEFT_CURLY_SINGLE_QUOTE, Chars.RIGHT_CURLY_SINGLE_QUOTE, Chars.LEFT_CURLY_SINGLE_QUOTE_LOW, Chars.LEFT_CURLY_DOUBLE_QUOTE, Chars.RIGHT_CURLY_DOUBLE_QUOTE, Chars.LEFT_CURLY_DOUBLE_QUOTE_LOW, Chars.LEFT_ANGLE_SINGLE_QUOTE, Chars.RIGHT_ANGLE_SINGLE_QUOTE, Chars.LEFT_ANGLE_DOUBLE_QUOTE, Chars.RIGHT_ANGLE_DOUBLE_QUOTE, Chars.LEFT_SQUARE_BRACKET, Chars.RIGHT_SQUARE_BRACKET, Chars.LEFT_ROUND_BRACKET, Chars.RIGHT_ROUND_BRACKET, Chars.LEFT_CURLY_BRACE, Chars.RIGHT_CURLY_BRACE];

        private static bool IsWordBoundary(char value)
        {
            UnicodeCategory category = char.GetUnicodeCategory(value);
            return !char.IsLetterOrDigit(value)
                && category is not UnicodeCategory.NonSpacingMark
                && category is not UnicodeCategory.SpacingCombiningMark
                && category is not UnicodeCategory.EnclosingMark;
        }

        /// <summary>Finds matching ranges using the selected search mode.</summary>
        /// <param name="phrase">The search phrase or terms.</param>
        /// <param name="text">The text to search.</param>
        /// <param name="matchCase">Whether matching is case-sensitive.</param>
        /// <param name="partialMatch">Whether matches may occur within a word.</param>
        /// <param name="options">The search mode.</param>
        /// <returns>Pairs containing the zero-based start index and length of each match.</returns>
        /// <remarks>Blank search phrases produce no matches. Word-based searches ignore empty terms between whitespace separators.</remarks>
        public static int[][] GetOccurrences(string phrase, string text, [Optional, DefaultParameterValue(false)] ref bool matchCase, [Optional, DefaultParameterValue(false)] ref bool partialMatch, [Optional, DefaultParameterValue(WordSearchOptions.AllWords)] ref WordSearchOptions options)
        {
            List<int[]> occurrences = [];

            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(phrase))
            {
                return [.. occurrences];
            }

            if (!matchCase)
            {
                phrase = phrase.ToLower();
                text = text.ToLower();
            }

            string[] words = options == WordSearchOptions.ExactPhrase
                ? [phrase]
                : phrase.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            int n = text.Length;

            List<int[]> occurrence = [];

            foreach (string word in words)
            {
                int m = word.Length;
                char lastChar = Chars.SPACE;
                int o = -1;
                int i = 0;
                int j = 0;

                while (i != n)
                {
                    char c = text[i];

                    if (j == m)
                    {
                        if (partialMatch || IsWordBoundary(c))
                        {
                            occurrence.Add([o, i - o]);
                        }

                        o = -1;
                        j = 0;
                    }

                    else if (c.Equals(word[j]))
                    {
                        if (o != -1)
                        {
                            j += 1;
                        }
                        else if (partialMatch || IsWordBoundary(lastChar))
                        {
                            o = i;
                            j += 1;
                        }
                        else
                        {
                            j = 0;
                        }
                    }

                    else
                    {
                        o = -1;
                        j = 0;
                    }

                    lastChar = c;
                    i += 1;
                }

                if (j == m)
                {
                    occurrence.Add([o, n - o]);
                }

                if (options.Equals(WordSearchOptions.AllWords) && occurrence.Count == 0)
                {
                    occurrences.Clear();
                    break;
                }

                foreach (int[] ia in occurrence)
                {
                    occurrences.Add(ia);
                }

                occurrence.Clear();
            }

            return [.. occurrences];
        }

        /// <summary>Starts a Windows shutdown with a one-second timeout.</summary>
        public static void ShutDown()
        {
            ProcessStartInfo psi = new("shutdown", "/s /t 1")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            _ = Process.Start(psi);
        }

        /// <summary>Identifies supported UI languages.</summary>
        public enum Language
        {
            /// <summary>German.</summary>
            [Value("de")]
            [GlobalStringValue("German")]
            German,
            /// <summary>English.</summary>
            [Value("en")]
            [GlobalStringValue("English")]
            English,
            /// <summary>Spanish.</summary>
            [Value("es")]
            [GlobalStringValue("Spanish")]
            Spanish,
            /// <summary>French.</summary>
            [Value("fr")]
            [GlobalStringValue("French")]
            French,
            /// <summary>Italian.</summary>
            [Value("it")]
            [GlobalStringValue("Italian")]
            Italian,
            /// <summary>Dutch.</summary>
            [Value("nl")]
            [GlobalStringValue("Dutch")]
            Dutch,
        }

        /// <summary>Associates an arbitrary value with a field.</summary>
        /// <param name="value">The value to associate with the field.</param>
        [AttributeUsage(AttributeTargets.Field)]
        public class ValueAttribute(object value) : Attribute
        {
            /// <summary>Gets the value associated with the field.</summary>
            public object Value { get; protected set; } = value;
        }

        /// <summary>Associates a localized resource string with a field.</summary>
        /// <param name="name">The name of the localized resource.</param>
        [AttributeUsage(AttributeTargets.Field)]
        public class GlobalStringValueAttribute(string name) : Attribute
        {
            /// <summary>Gets the localized string, or an empty string if the resource is missing.</summary>
            public string Value { get; protected set; } = Globals.ResourceManager.GetString(name) is string s ? s : string.Empty;
        }

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyIcon(IntPtr handle);

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
                timer.Tag = null;
                timer.Dispose();
                await function();
            }
        }

        /// <summary>Schedules an action using a Windows Forms timer.</summary>
        /// <param name="action">The action to run.</param>
        /// <param name="interval">The delay in milliseconds.</param>
        public static void Delay(Action action, int interval = 1000)
        {
            System.Windows.Forms.Timer timer = new() { Interval = interval, Enabled = true, Tag = action };
            timer.Tick += Timer_Tick;
        }

        /// <summary>Schedules an asynchronous function using a Windows Forms timer.</summary>
        /// <param name="function">The asynchronous function to run.</param>
        /// <param name="interval">The delay in milliseconds.</param>
        /// <remarks>The timer is disposed before invoking the function, so each scheduled call runs once.</remarks>
        public static void DelayAsync(Func<Task> function, int interval = 1000)
        {
            System.Windows.Forms.Timer timer = new() { Interval = interval, Tag = function };
            timer.Tick += Timer_TickAsync;
            timer.Start();
        }

        /// <summary>Schedules an action when the task completes, faults, or is canceled.</summary>
        /// <param name="task">The task to observe.</param>
        /// <param name="action">The action to run.</param>
        public static async void OnTaskCompleted(Task task, Action action)
        {
            using Task tc = task.ContinueWith((result) => { action(); });
            await tc;
        }

        /// <summary>Runs an action on the thread pool with cancellation before execution.</summary>
        /// <param name="action">The action to run.</param>
        /// <param name="cancellationTokenSource">The source whose token is passed to task scheduling. This method disposes the source at completion.</param>
        /// <param name="onCompletion">The optional callback run after cleanup.</param>
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

        /// <summary>Checks whether the path has the file system reparse-point attribute.</summary>
        /// <param name="path">The file system path.</param>
        public static bool IsSymbolicLink(string path) => File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);

        /// <summary>Checks whether the path identifies an existing directory without the reparse-point attribute.</summary>
        /// <param name="path">The file system path.</param>
        public static bool IsValidDirectoryPath(string path) => !string.IsNullOrEmpty(path) && Directory.Exists(path) && !IsSymbolicLink(path);

        /// <summary>Checks whether the path identifies an existing file without the reparse-point attribute.</summary>
        /// <param name="path">The file system path.</param>
        public static bool IsValidFilePath(string path) => !string.IsNullOrEmpty(path) && File.Exists(path) && !IsSymbolicLink(path);

        /// <summary>Checks whether the path identifies an existing directory or an existing file without the reparse-point attribute.</summary>
        /// <param name="path">The file system path.</param>
        public static bool IsValidPath(string path) => (!string.IsNullOrEmpty(path) && Directory.Exists(path)) || (File.Exists(path) && !IsSymbolicLink(path));

        /// <summary>Checks whether the path has the directory attribute.</summary>
        /// <param name="path">The file system path.</param>
        public static bool IsDirectory(string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);

        /// <summary>Checks whether the drive is expected to support native file system watching.</summary>
        /// <param name="path">The file system path.</param>
        /// <remarks>The check treats a fixed drive without an associated partition as unsupported. Unrecognized drives are treated as supported.</remarks>
        public static bool SupportsFileSystemWatcher(string path)
        {
            const string DRIVE_TYPE_PROPERTY = "DriveType";
            const string LOGICAL_DISK_QUERY = "SELECT DriveType FROM Win32_LogicalDisk WHERE DeviceID = '{0}'";
            const string LOGICAL_DISK_TO_PARTITION_QUERY = "ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{0}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";

            string? root = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root) || root.StartsWith(@"\\"))
            {
                return true;
            }

            try
            {
                string deviceId = root.TrimEnd(Path.DirectorySeparatorChar);
                using ManagementObjectSearcher searcher = new(string.Format(LOGICAL_DISK_QUERY, deviceId));
                foreach (ManagementObject volume in searcher.Get().Cast<ManagementObject>())
                {
                    using (volume)
                    {
                        uint driveType = Convert.ToUInt32(volume[DRIVE_TYPE_PROPERTY]);
                        if (driveType != 3)
                        {
                            return true;
                        }
                    }

                    using ManagementObjectSearcher partitionSearcher = new(string.Format(LOGICAL_DISK_TO_PARTITION_QUERY, deviceId));
                    foreach (ManagementObject partition in partitionSearcher.Get().Cast<ManagementObject>())
                    {
                        partition.Dispose();
                        return true;
                    }
                    return false;
                }
            }
            catch { }
            return true;
        }

        /// <summary>Returns the flags with the specified bits set or cleared.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="flags">The initial set of flags.</param>
        /// <param name="flag">The bits to set or clear.</param>
        /// <param name="enabled">Whether to set the bits. Otherwise, they are cleared.</param>
        public static T GetToggledFlags<T>(T flags, T flag, bool enabled) where T : struct, Enum
        {
            long flagsValue = Convert.ToInt64(flags);
            long flagValue = Convert.ToInt64(flag);
            long value = enabled ? flagsValue | flagValue : flagsValue & ~flagValue;
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>Reads a string property from a JSON object, returning an empty string if absent or of another kind.</summary>
        /// <param name="root">The JSON object to inspect.</param>
        /// <param name="propertyName">The JSON property name.</param>
        public static string GetString(JsonElement root, string propertyName)
            => root.TryGetProperty(propertyName, out JsonElement element) && element.ValueKind == JsonValueKind.String
                ? element.GetString() ?? EMPTY_STRING
                : EMPTY_STRING;

        /// <summary>Reads a JSON number or an invariant numeric string, returning null if neither can be read.</summary>
        /// <param name="root">The JSON object to inspect.</param>
        /// <param name="propertyName">The JSON property name.</param>
        public static double? GetDouble(JsonElement root, string propertyName)
        {
            return !root.TryGetProperty(propertyName, out JsonElement element)
                ? null
                : element.ValueKind switch
                {
                    JsonValueKind.Number when element.TryGetDouble(out double value) => value,
                    JsonValueKind.String when double.TryParse(element.GetString(), System.Globalization.NumberStyles.Float, CULTURE_INFO_DEFAULT, out double value) => value,
                    _ => null
                };
        }

        /// <summary>Reads and parses a date and optional time from JSON string properties.</summary>
        /// <param name="root">The JSON object to inspect.</param>
        /// <param name="datePropertyName">The name of the property containing the date.</param>
        /// <param name="timePropertyName">The name of the optional property containing the time.</param>
        public static DateTime? GetDateTime(JsonElement root, string datePropertyName, string? timePropertyName = null)
        {
            string value = GetString(root, datePropertyName);
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (timePropertyName is string timeProperty && GetString(root, timeProperty) is string time && !string.IsNullOrEmpty(time))
            {
                value = $"{value} {time}";
            }
            string[] formats =
            [
                "yyyy:MM:dd HH:mm:ss",
                "yyyy:MM:dd HH:mm:ssK",
                "yyyy:MM:dd",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd"
            ];
            return DateTime.TryParseExact(value, formats, CULTURE_INFO_DEFAULT, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime exact)
                ? exact
                : value.AsDateTime();
        }

        /// <summary>Resolves the target of a file system link, or returns null if it cannot be resolved.</summary>
        /// <param name="path">The file system path.</param>
        public static string? GetTargetFile(string path)
        {
            try
            {
                FileInfo fileInfo = new(path);
                return fileInfo.ResolveLinkTarget(true)?.FullName ?? GetFullLinkTarget(fileInfo);
            }
            catch { return null; }
        }

        /// <summary>Resolves the target of a directory link, or returns null if it cannot be resolved.</summary>
        /// <param name="path">The file system path.</param>
        public static string? GetTargetDirectory(string path)
        {
            try
            {
                DirectoryInfo directoryInfo = new(path);
                return directoryInfo.ResolveLinkTarget(true)?.FullName ?? GetFullLinkTarget(directoryInfo);
            }
            catch { return null; }
        }

        private static string? GetFullLinkTarget(FileSystemInfo fileSystemInfo)
        {
            string? linkTarget = fileSystemInfo.LinkTarget;
            return string.IsNullOrEmpty(linkTarget)
                ? null
                : Path.IsPathRooted(linkTarget)
                ? linkTarget
                : Path.GetFullPath(Path.Join(fileSystemInfo is FileInfo fi ? fi.DirectoryName : ((DirectoryInfo)fileSystemInfo).Parent?.FullName ?? string.Empty, linkTarget));
        }

        /// <summary>Gets the file name, using an untitled name when the path has no name or only an extension.</summary>
        /// <param name="path">The file system path.</param>
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

        /// <summary>Gets the file name without its extension, using the localized default if the name is empty.</summary>
        /// <param name="path">The file system path.</param>
        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path) is string n && !string.IsNullOrEmpty(n) ? n : Globals.FileNameDefault;
        }

        /// <summary>Combines the parent path with the file name without its extension.</summary>
        /// <param name="path">The file system path.</param>
        public static string GetPathWithoutExtension(string path)
        {
            string nwe = GetFileNameWithoutExtension(path);
            string pp = GetParentPath(path);
            return string.IsNullOrEmpty(pp) ? nwe : Path.Join(pp, nwe);
        }

        /// <summary>Gets the extension, including the leading dot, in lowercase.</summary>
        /// <param name="path">The file system path.</param>
        public static string GetExtension(string path) => Path.GetExtension(path) is string e ? e.ToLower() : string.Empty;

        /// <summary>Gets the directory portion of the supplied path, or an empty string if absent.</summary>
        /// <param name="path">The file system path.</param>
        public static string GetDirectoryPath(string path) => Path.GetDirectoryName(path) is string dp ? dp : string.Empty;

        /// <summary>Gets the last component of the supplied path, or an empty string if absent.</summary>
        /// <param name="path">The file system path.</param>
        public static string GetDirectoryName(string path) => Path.GetFileName(path) is string fp ? fp : string.Empty;

        /// <summary>Gets the directory portion of the path, or the root if the directory portion is unavailable.</summary>
        /// <param name="path">The file system path.</param>
        public static string GetParentPath(string path)
        {
            return Path.GetDirectoryName(path) is string dp ? dp : Directory.GetDirectoryRoot(path) is string dr ? dr : string.Empty;
        }

        /// <summary>Searches for an existing parent directory, or selects a default documents path for empty input.</summary>
        /// <param name="path">The file system path.</param>
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

        /// <summary>Returns the portion of the full path following the parent directory prefix used by this method.</summary>
        /// <param name="parentPath">The parent path used to derive the result.</param>
        /// <param name="fullPath">The full path to examine.</param>
        public static string GetRelativePath(string parentPath, string fullPath)
        {
            if (parentPath.SplitLast(Path.DirectorySeparatorChar) is string[] sa1)
            {
                parentPath = sa1.First();
            }
            return fullPath.SplitFirst($"{parentPath}{Path.DirectorySeparatorChar}") is string[] sa2 ? sa2.Last() : fullPath;
        }

        /// <summary>Finds an unused candidate path by adding a numbered suffix when necessary.</summary>
        /// <param name="path">The file system path.</param>
        /// <param name="excludedPaths">The paths to avoid, or null to inspect the parent directory.</param>
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

        /// <summary>Combines the parent path with the file name after removing a recognized duplicate suffix.</summary>
        /// <param name="path">The file system path.</param>
        public static string GetRawPath(string path) => Path.Join(GetParentPath(path), GetRawName(path));

        /// <summary>Gets the file name with its lowercase extension and without a recognized duplicate suffix.</summary>
        /// <param name="path">The file system path.</param>
        public static string GetRawName(string path) => $"{GetRawNameWithoutExtension(path)}{GetExtension(path)}";

        /// <summary>Gets the file name without its extension or recognized duplicate suffix.</summary>
        /// <param name="path">The file system path.</param>
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

        /// <summary>Returns distinct paths after removing recognized duplicate suffixes.</summary>
        /// <param name="paths">The paths to normalize.</param>
        public static string[] GetRawPaths(IEnumerable<string> paths)
        {
            HashSet<string> hs = [];
            foreach (string path in paths)
            {
                _ = hs.Add(GetRawPath(path));
            }
            return [.. hs];
        }

        /// <summary>Lists child directories. Recursive mode also includes their immediate child directories.</summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="recursive">Whether to include paths below the immediate children.</param>
        /// <remarks>Enumeration errors are suppressed.</remarks>
        public static string[] GetDirectoryPaths(string directoryPath, bool recursive = false)
        {
            string[] paths = [];
            try
            {
                List<string> l = [.. Directory.EnumerateDirectories(directoryPath, "*.*", SearchOption.TopDirectoryOnly)];
                paths = [.. l];
                if (recursive)
                {
                    foreach (string dp in paths)
                    {
                        l.AddRange(GetDirectoryPaths(dp));
                    }
                    paths = [.. l];
                }
                else
                {
                    Array.Sort(paths);
                }
            }
            catch { }
            return paths;
        }

        /// <summary>Lists files in the directory and optionally its subdirectories.</summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="recursive">Whether to include paths below the immediate children.</param>
        /// <remarks>Enumeration errors result in an empty array.</remarks>
        public static string[] GetFilePaths(string directoryPath, bool recursive = false)
        {
            try
            {
                SearchOption so = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] paths = [.. Directory.EnumerateFiles(directoryPath, "*.*", so)];
                if (!recursive)
                {
                    Array.Sort(paths);
                }
                return paths;
            }
            catch { return []; }
        }

        /// <summary>Lists directory paths followed by file paths.</summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="recursive">Whether to include paths below the immediate children.</param>
        public static string[] GetAllPaths(string directoryPath, bool recursive = false)
        {
            string[] directoryPaths = GetDirectoryPaths(directoryPath, recursive);
            string[] filePaths = GetFilePaths(directoryPath, recursive);
            return [.. directoryPaths.Concat(filePaths)];
        }

        /// <summary>Returns the matching system special-folder path, or null if none matches.</summary>
        /// <param name="directoryPath">The directory path.</param>
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

        /// <summary>Returns the paths reported for the system special-folder values.</summary>
        public static string[] GetSpecialfolders()
        {
            List<string> l = [];
            foreach (Environment.SpecialFolder specialFolder in Enum.GetValues<Environment.SpecialFolder>())
            {
                l.Add(Environment.GetFolderPath(specialFolder));
            }
            return [.. l];
        }

        /// <summary>Attempts to delete a file.</summary>
        /// <param name="path">The file to delete.</param>
        /// <returns>True if deletion completes without an exception, including when the file is already absent.</returns>
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

        /// <summary>Attempts to delete a directory and all its contents.</summary>
        /// <param name="path">The directory to delete.</param>
        /// <returns>True if the directory is deleted.</returns>
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

        /// <summary>Attempts to move a file, creating the destination parent directory if needed.</summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
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
                _ = Directory.CreateDirectory(pp);
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
                    _ = DeleteDirectory(pp);
                }
                return false;
            }
        }

        /// <summary>Attempts to move a directory.</summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <remarks>Creates the destination parent directory if needed. An existing destination is not replaced.</remarks>
        public static bool MoveDirectory(string sourcePath, string destinationPath)
        {
            if (sourcePath == destinationPath)
            {
                return false;
            }
            try
            {
                string destination = Path.TrimEndingDirectorySeparator(Path.GetFullPath(destinationPath));
                if (Path.GetDirectoryName(destination) is string parent)
                {
                    _ = Directory.CreateDirectory(parent);
                }
                Directory.Move(sourcePath, destination);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Attempts to copy a file without replacing an existing destination.</summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
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
                _ = Directory.CreateDirectory(pp);
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
                    _ = DeleteDirectory(pp);
                }
                return false;
            }
        }

        /// <summary>Copies a directory tree, including empty directories, without overwriting files.</summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <returns>True if the entire copy completes. A failure can leave files already copied in the destination.</returns>
        public static bool CopyDirectory(string sourcePath, string destinationPath)
        {
            try
            {
                string source = Path.TrimEndingDirectorySeparator(Path.GetFullPath(sourcePath));
                string destination = Path.TrimEndingDirectorySeparator(Path.GetFullPath(destinationPath));
                string sourcePrefix = Path.EndsInDirectorySeparator(source) ? source : source + Path.DirectorySeparatorChar;
                if (!Directory.Exists(source)
                    || string.Equals(source, destination, StringComparison.OrdinalIgnoreCase)
                    || destination.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                _ = Directory.CreateDirectory(destination);
                foreach (string directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
                {
                    _ = Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
                }
                foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
                {
                    File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Attempts to move a file to a new name while retaining its extension.</summary>
        /// <param name="path">The file to rename.</param>
        /// <param name="name">The new name without the extension.</param>
        /// <param name="parentPath">The destination directory, or an empty string to use the current parent.</param>
        /// <returns>The new path on success, or the original path if no move occurs.</returns>
        public static string RenameFile(string path, string name, string parentPath = EMPTY_STRING)
        {
            if (string.IsNullOrEmpty(parentPath))
            {
                parentPath = GetParentPath(path);
            }
            else if (!Directory.Exists(parentPath))
            {
                _ = Directory.CreateDirectory(parentPath);
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

        /// <summary>Compresses UTF-8 text using GZip.</summary>
        /// <param name="s">The text to compress.</param>
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

        /// <summary>Creates a ZIP archive with the supplied files at its root.</summary>
        /// <param name="archivePath">The archive to create or replace.</param>
        /// <param name="filePaths">The files to include. Their file names must be unique, ignoring case.</param>
        /// <remarks>An existing archive is replaced only after the new archive has been written.</remarks>
        public static void Zipped(string archivePath, IEnumerable<string> filePaths)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
            ArgumentNullException.ThrowIfNull(filePaths);

            string[] paths = [.. filePaths];
            if (paths.Length == 0)
            {
                throw new ArgumentException("At least one file is required.", nameof(filePaths));
            }

            string[] entryNames = [.. paths.Select(path => Path.GetFileName(path)
                ?? throw new ArgumentException("File paths must include a file name.", nameof(filePaths)))];
            if (entryNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() != entryNames.Length)
            {
                throw new ArgumentException("Archive entry names must be unique.", nameof(filePaths));
            }

            string fullArchivePath = Path.GetFullPath(archivePath);
            string temporaryPath = fullArchivePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                using (FileStream stream = new(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (ZipArchive archive = new(stream, ZipArchiveMode.Create))
                {
                    foreach (string path in paths)
                    {
                        _ = archive.CreateEntryFromFile(path, Path.GetFileName(path), CompressionLevel.Optimal);
                    }
                }

                File.Move(temporaryPath, fullArchivePath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        /// <summary>Decompresses GZip data and decodes the result as UTF-8.</summary>
        /// <param name="bytes">The compressed data.</param>
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

        /// <summary>Extracts a ZIP archive and returns the paths of the extracted files.</summary>
        /// <param name="archivePath">The ZIP archive to read.</param>
        /// <param name="outputDirectory">The destination directory.</param>
        /// <remarks>Existing files at the same paths are overwritten.</remarks>
        public static string[] Unzipped(string archivePath, string outputDirectory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

            string destinationRoot = Path.GetFullPath(outputDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            _ = Directory.CreateDirectory(destinationRoot);

            List<string> extractedFiles = [];
            using ZipArchive archive = ZipFile.OpenRead(archivePath);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string destinationPath = Path.GetFullPath(Path.Combine(destinationRoot, entry.FullName));
                if (!destinationPath.StartsWith(destinationRoot, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"Archive entry '{entry.FullName}' is outside the destination directory.");
                }

                if (string.IsNullOrEmpty(entry.Name))
                {
                    _ = Directory.CreateDirectory(destinationPath);
                    continue;
                }

                string parentPath = Path.GetDirectoryName(destinationPath)
                    ?? throw new InvalidDataException($"Archive entry '{entry.FullName}' has no destination directory.");
                _ = Directory.CreateDirectory(parentPath);
                entry.ExtractToFile(destinationPath, true);
                extractedFiles.Add(destinationPath);
            }

            return [.. extractedFiles];
        }

        /// <summary>Creates an icon from a bitmap file.</summary>
        /// <param name="path">The path to the bitmap file.</param>
        /// <returns>An independent icon that the caller must dispose.</returns>
        public static Icon GetIcon(string path)
        {
            using Bitmap bitmap = new(path);
            IntPtr handle = bitmap.GetHicon();
            try
            {
                using Icon icon = Icon.FromHandle(handle);
                return (Icon)icon.Clone();
            }
            finally
            {
                _ = DestroyIcon(handle);
            }
        }

        /// <summary>Attempts to launch an existing executable.</summary>
        /// <param name="path">The executable path.</param>
        /// <exception cref="IOException">The file does not exist.</exception>
        /// <exception cref="ArgumentException">The file does not have the executable extension.</exception>
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

        /// <summary>Sets the minimum size of each control to the largest width and height in the collection.</summary>
        /// <param name="controlCollection">A nonempty collection of controls.</param>
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

        /// <summary>Estimates a layout size for displaying text.</summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font to use, or null to use the library default.</param>
        /// <param name="dimensions">The proposed dimensions used for measuring and choosing the aspect ratio.</param>
        /// <param name="padding">Optional padding added to the library default.</param>
        /// <param name="format">The text rendering flags used for measuring.</param>
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
            double ratioScale = Math.Sqrt(r);
            int w = (int)Math.Ceiling((sqr * ratioScale) + Math.Sqrt(p.Horizontal));
            int textWidth = Math.Max(1, w - p.Horizontal);
            int h = TextRenderer.MeasureText(text, font, new(textWidth, int.MaxValue), format).Height + p.Vertical;
            return new(w, h);
        }

        /// <summary>Returns the widest measured item text with margin, subject to the supplied minimum.</summary>
        /// <param name="objects">The items to measure.</param>
        /// <param name="font">The measurement font.</param>
        /// <param name="marginHorizontal">The horizontal margin in pixels.</param>
        /// <param name="minWidth">The minimum result width in pixels.</param>
        public static int GetMaxWidth(object[] objects, Font font, int marginHorizontal = 0, int minWidth = 0)
        {
            int marginOfError = 2;
            List<int> l = [marginHorizontal + marginOfError, minWidth];
            _label.Font = font;
            foreach (object o in objects)
            {
                _label.Text = $"{o}";
                l.Add(_label.PreferredWidth + marginHorizontal + marginOfError);
            }
            return l.Max();
        }

        /// <summary>Returns the tallest measured item text with margin, subject to the supplied minimum.</summary>
        /// <param name="objects">The items to measure.</param>
        /// <param name="font">The measurement font.</param>
        /// <param name="marginVertical">The vertical margin in pixels.</param>
        /// <param name="minHeight">The minimum result height in pixels.</param>
        public static int GetMaxHeight(object[] objects, Font font, int marginVertical = 0, int minHeight = 0)
        {
            int marginOfError = 2;
            List<int> l = [marginVertical + marginOfError, minHeight];
            _label.Font = font;
            foreach (object o in objects)
            {
                _label.Text = $"{o}";
                l.Add(_label.PreferredHeight + marginVertical + marginOfError);
            }
            return l.Max();
        }

        /// <summary>Returns the maximum measured width and height across the item text.</summary>
        /// <param name="objects">The items to measure.</param>
        /// <param name="font">The measurement font.</param>
        /// <param name="margin">The margin added to the measurements.</param>
        /// <param name="minSize">The optional minimum width and height.</param>
        public static Size GetMaxSize(object[] objects, Font font, Padding margin, Size? minSize = null)
        {
            minSize ??= new();
            int mw = minSize.Value.Width;
            int mh = minSize.Value.Height;
            return new(GetMaxWidth(objects, font, margin.Horizontal, mw), GetMaxHeight(objects, font, margin.Vertical, mh));
        }

        /// <summary>Calculates column and row counts for arranging items in a grid.</summary>
        /// <param name="numItems">The number of items.</param>
        /// <param name="landscape">Whether to add extra columns rather than extra rows.</param>
        public static Size GetTableLayout(int numItems, bool landscape = true)
        {
            int u = (int)Math.Sqrt(numItems);
            int r = numItems - (int)Math.Pow(u, 2);
            int w = landscape ? u + r : u;
            int h = landscape ? u : u + r;
            return new(w, h);
        }

        /// <summary>Extracts URL matches and removes trailing periods.</summary>
        /// <param name="text">The text to scan.</param>
        public static string[] GetUrls(string text)
        {
            List<string> l = [];
            Regex regex = CreateUrlRegex();
            l.AddRange(regex.Matches(text).Select(match => match.Value.TrimEnd(PERIOD)));
            return [.. l];
        }

        /// <summary>Creates link ranges for the first occurrence of each label in the text.</summary>
        /// <param name="text">The label text.</param>
        /// <param name="textLinkPairs">The display text and destinations, or null to detect URLs automatically.</param>
        public static LinkLabel.Link[] GetLinkLabelLinks(string text, Dictionary<string, string>? textLinkPairs = null)
        {
            List<LinkLabel.Link> l = [];
            textLinkPairs ??= GetUrls(text).Distinct().ToDictionary(x => x);
            foreach (KeyValuePair<string, string> kvp in textLinkPairs)
            {
                int textIndex = text.IndexOf(kvp.Key);
                if (textIndex < 0)
                {
                    continue;
                }
                int linkIndex = textIndex - text[..textIndex].Count(character => character == '\r');
                l.Add(new(linkIndex, kvp.Key.Length) { LinkData = kvp.Value });
            }
            return [.. l];
        }

        /// <summary>Selects the requested language code if supported, or the second configured code otherwise.</summary>
        /// <param name="globals">The supported language codes and associated values.</param>
        /// <param name="cultureInfo">The preferred culture, or null to use the current culture. The invariant culture also selects the current culture.</param>
        public static string GetValidTwoLetterISOLanguageName(Dictionary<string, string[]> globals, CultureInfo? cultureInfo)
        {
            if (cultureInfo == null || cultureInfo == CULTURE_INFO_DEFAULT)
            {
                cultureInfo = CultureInfo.CurrentCulture;
            }
            string n = cultureInfo.TwoLetterISOLanguageName;
            string[] supportedLanguages = [.. globals.Keys];
            if (!supportedLanguages.Contains(n))
            {
                n = supportedLanguages[1];
            }
            return n;
        }

        /// <summary>Writes resource bytes to a temporary file if the requested name does not already exist.</summary>
        /// <param name="name">The temporary file name.</param>
        /// <param name="resource">The bytes to write if the file is absent.</param>
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

        /// <summary>Sets the tooltip text for a control.</summary>
        /// <param name="control">The control to associate with the tooltip.</param>
        /// <param name="caption">The tooltip text.</param>
        public static void AddToolTip(this Control control, string caption) => _toolTip.SetToolTip(control, caption);

        /// <summary>Sets the initial tooltip delay for controls.</summary>
        /// <param name="initialDelay">The initial delay in milliseconds, with a minimum of one.</param>
        public static void SetToolTipInitialDelay(int initialDelay) =>
            _toolTip.InitialDelay = Math.Max(initialDelay, 1);

        /// <summary>Sets the initial tooltip delay for tool strip items.</summary>
        /// <param name="initialDelay">The initial delay in milliseconds, with a minimum of one.</param>
        public static void SetToolStripToolTipInitialDelay(int initialDelay)
        {
            int delay = Math.Max(initialDelay, 1);
            _toolStripToolTip.InitialDelay = delay;
            _toolStripToolTipTimer.Interval = delay;
        }

        /// <summary>Sets the initial tooltip delay for tab headers.</summary>
        /// <param name="initialDelay">The initial delay in milliseconds, with a minimum of one.</param>
        public static void SetTabToolTipInitialDelay(int initialDelay)
        {
            _tabToolTipInitialDelay = Math.Max(initialDelay, 1);
            _tabToolTipTimer.Interval = _tabToolTipInitialDelay;
            foreach (TabControl tabControl in _tabToolTips.Keys.ToArray())
            {
                if (_tabToolTips[tabControl] != null)
                {
                    _tabToolTips[tabControl] = _tabToolTipInitialDelay;
                }
            }
        }

        /// <summary>Enables or disables custom tooltips for a tab control and disables the built-in tooltips.</summary>
        /// <param name="tabControl">The tab control to configure.</param>
        /// <param name="enabled">Whether custom tooltips are enabled.</param>
        public static void SetTabToolTips(this TabControl tabControl, bool enabled)
        {
            tabControl.ShowToolTips = false;
            if (!_tabToolTips.ContainsKey(tabControl))
            {
                tabControl.MouseMove += TabControl_MouseMove;
                tabControl.MouseLeave += TabControl_MouseLeave;
                tabControl.Disposed += TabControl_Disposed;
            }
            _tabToolTips[tabControl] = enabled ? _tabToolTipInitialDelay : null;

            if (!_tabToolTipTimerConfigured)
            {
                _tabToolTipTimer.Tick += TabToolTipTimer_Tick;
                _tabToolTipTimerConfigured = true;
            }

            if (!enabled && ReferenceEquals(_pendingTabToolTip, tabControl))
            {
                HideTabToolTip(tabControl);
            }
        }

        private static void TabControl_MouseMove(object? sender, MouseEventArgs e)
        {
            if (sender is not TabControl tabControl
                || !_tabToolTips.TryGetValue(tabControl, out int? initialDelay)
                || initialDelay == null)
            {
                return;
            }

            int tabIndex = -1;
            for (int index = 0; index < tabControl.TabCount; index++)
            {
                if (tabControl.GetTabRect(index).Contains(e.Location))
                {
                    tabIndex = index;
                    break;
                }
            }

            if (ReferenceEquals(_pendingTabToolTip, tabControl) && _pendingTabIndex == tabIndex)
            {
                return;
            }

            HideTabToolTip(_pendingTabToolTip);
            if (tabIndex >= 0 && !string.IsNullOrEmpty(tabControl.TabPages[tabIndex].ToolTipText))
            {
                _pendingTabToolTip = tabControl;
                _pendingTabIndex = tabIndex;
                _tabToolTipTimer.Interval = initialDelay.Value;
                _tabToolTipTimer.Start();
            }
        }

        private static void TabControl_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is TabControl tabControl)
            {
                HideTabToolTip(tabControl);
            }
        }

        private static void TabControl_Disposed(object? sender, EventArgs e)
        {
            if (sender is TabControl tabControl)
            {
                HideTabToolTip(tabControl);
                _ = _tabToolTips.Remove(tabControl);
            }
        }

        private static void TabToolTipTimer_Tick(object? sender, EventArgs e)
        {
            _tabToolTipTimer.Stop();
            if (_pendingTabToolTip is TabControl tabControl
                && _pendingTabIndex >= 0
                && _pendingTabIndex < tabControl.TabCount)
            {
                Point location = tabControl.PointToClient(Cursor.Position);
                location.Offset(12, 20);
                _toolTip.Show(tabControl.TabPages[_pendingTabIndex].ToolTipText, tabControl, location, TOOLTIP_AUTO_POP_DELAY);
            }
        }

        private static void HideTabToolTip(TabControl? tabControl)
        {
            _tabToolTipTimer.Stop();
            if (tabControl != null)
            {
                _toolTip.Hide(tabControl);
            }
            _pendingTabToolTip = null;
            _pendingTabIndex = -1;
        }

        /// <summary>Registers a custom tooltip for a tool strip item and disables its built-in tooltip.</summary>
        /// <param name="item">The tool strip item.</param>
        /// <param name="caption">The tooltip text.</param>
        public static void AddToolTip(this ToolStripItem item, string caption)
        {
            item.AutoToolTip = false;
            item.ToolTipText = string.Empty;
            _toolStripToolTips[item] = caption;

            if (!_toolStripToolTipTimerConfigured)
            {
                _toolStripToolTipTimer.Tick += ToolStripToolTipTimer_Tick;
                _toolStripToolTipTimerConfigured = true;
            }

            item.MouseEnter -= ToolStripItem_MouseEnter;
            item.MouseEnter += ToolStripItem_MouseEnter;
            item.MouseLeave -= ToolStripItem_MouseLeave;
            item.MouseLeave += ToolStripItem_MouseLeave;
            item.Disposed -= ToolStripItem_Disposed;
            item.Disposed += ToolStripItem_Disposed;
        }

        private static void ToolStripItem_MouseEnter(object? sender, EventArgs e)
        {
            _toolStripToolTipTimer.Stop();
            _pendingToolStripToolTip = sender as ToolStripItem;
            if (_pendingToolStripToolTip != null
                && _toolStripToolTips.TryGetValue(_pendingToolStripToolTip, out string? caption)
                && !string.IsNullOrEmpty(caption))
            {
                _toolStripToolTipTimer.Start();
            }
        }

        private static void ToolStripItem_MouseLeave(object? sender, EventArgs e)
        {
            _toolStripToolTipTimer.Stop();
            _pendingToolStripToolTip = null;
            if (sender is ToolStripItem item && item.Owner != null)
            {
                _toolStripToolTip.Hide(item.Owner);
            }
        }

        private static void ToolStripItem_Disposed(object? sender, EventArgs e)
        {
            if (sender is ToolStripItem item)
            {
                _ = _toolStripToolTips.Remove(item);
            }
        }

        private static void ToolStripToolTipTimer_Tick(object? sender, EventArgs e)
        {
            _toolStripToolTipTimer.Stop();
            if (_pendingToolStripToolTip is ToolStripItem item
                && item.Owner is ToolStrip owner
                && _toolStripToolTips.TryGetValue(item, out string? caption)
                && !string.IsNullOrEmpty(caption))
            {
                Point location = owner.PointToClient(Cursor.Position);
                location.Offset(12, 20);
                _toolStripToolTip.Show(caption, owner, location, TOOLTIP_AUTO_POP_DELAY);
            }
        }

        /// <summary>Sets the shared tooltip colors and enables owner drawing.</summary>
        /// <param name="foreColor">The tooltip text color.</param>
        /// <param name="backColor">The tooltip background color.</param>
        public static void SetToolTipColors(Color foreColor, Color backColor)
        {
            _toolTip.ForeColor = foreColor;
            _toolTip.BackColor = backColor;
            _toolStripToolTip.ForeColor = foreColor;
            _toolStripToolTip.BackColor = backColor;
            if (_toolTipOwnerDrawConfigured)
            {
                return;
            }

            _toolTip.OwnerDraw = true;
            _toolTip.Draw += ToolTip_Draw;
            _toolStripToolTip.OwnerDraw = true;
            _toolStripToolTip.Draw += ToolTip_Draw;
            _toolTipOwnerDrawConfigured = true;
        }

        private static void ToolTip_Draw(object? sender, DrawToolTipEventArgs e)
        {
            ToolTip toolTip = sender as ToolTip ?? _toolTip;
            using SolidBrush background = new(toolTip.BackColor);
            e.Graphics.FillRectangle(background, e.Bounds);
            ControlPaint.DrawBorder(e.Graphics, e.Bounds, ControlPaint.Dark(toolTip.BackColor), ButtonBorderStyle.Solid);
            Rectangle textBounds = Rectangle.Inflate(e.Bounds, -4, -2);
            TextRenderer.DrawText(
                e.Graphics,
                e.ToolTipText,
                e.Font,
                textBounds,
                toolTip.ForeColor,
                TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter);
        }

        /// <summary>Creates a thumbnail at the supplied bitmap's dimensions.</summary>
        /// <param name="bmp">The source bitmap, or null to return null.</param>
        /// <returns>The thumbnail image, which the caller must dispose, or null.</returns>
        public static Image? GetThumbnailImage(Bitmap? bmp) => bmp?.GetThumbnailImage(bmp.Width, bmp.Height, null, IntPtr.Zero);

        /// <summary>Disposes the shared tooltip components, tab tooltip timer, and measurement label.</summary>
        public static void Dispose()
        {
            Debug.WriteLine($"Disposing 'Utils'...");

            _toolTip.Active = false;
            _toolTip.RemoveAll();
            _toolTip.Dispose();
            _toolStripToolTip.Active = false;
            _toolStripToolTip.RemoveAll();
            _toolStripToolTip.Dispose();
            _tabToolTipTimer.Stop();
            _tabToolTipTimer.Dispose();
            _tabToolTips.Clear();
            _label.Dispose();
        }

        [GeneratedRegex(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase, "nl-BE")]
        private static partial Regex CreateUrlRegex();
    }
}
