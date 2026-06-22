namespace WinFormsLib
{
    public sealed class PollingFileSystemWatcher : IDisposable
    {
        private readonly record struct FileSystemEntryState(string Path, FileAttributes Attributes, long Length, long LastWriteTimeUtcTicks);

        private readonly System.Windows.Forms.Timer _timer = new();
        private FileSystemEntryState[] _snapshot = [];
        private string _path = string.Empty;
        private bool _directoryExists;
        private bool _initialized;
        private bool _disposed;

        public event EventHandler? Changed;

        public PollingFileSystemWatcher()
        {
            _timer.Tick += Timer_Tick;
        }

        public string Path
        {
            get => _path;
            set
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (string.Equals(_path, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                _path = value;
                if (EnableRaisingEvents)
                {
                    InitializeSnapshot();
                }
            }
        }

        public int Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public bool EnableRaisingEvents
        {
            get => _timer.Enabled;
            set
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (value)
                {
                    InitializeSnapshot();
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        private void InitializeSnapshot()
        {
            _initialized = TryGetSnapshot(_path, out _directoryExists, out _snapshot);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!TryGetSnapshot(_path, out bool directoryExists, out FileSystemEntryState[] snapshot))
            {
                return;
            }

            bool changed = _initialized
                && (_directoryExists != directoryExists || !_snapshot.SequenceEqual(snapshot));
            _directoryExists = directoryExists;
            _snapshot = snapshot;
            _initialized = true;

            if (changed)
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private static bool TryGetSnapshot(string path, out bool directoryExists, out FileSystemEntryState[] snapshot)
        {
            directoryExists = Directory.Exists(path);
            if (!directoryExists)
            {
                snapshot = [];
                return true;
            }

            try
            {
                snapshot = [.. Directory.EnumerateFileSystemEntries(path).Select(entryPath =>
                {
                    FileAttributes attributes = File.GetAttributes(entryPath);
                    long length = attributes.HasFlag(FileAttributes.Directory) ? 0 : new FileInfo(entryPath).Length;
                    long lastWriteTimeUtcTicks = File.GetLastWriteTimeUtc(entryPath).Ticks;
                    return new FileSystemEntryState(entryPath, attributes, length, lastWriteTimeUtcTicks);
                }).OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)];
                return true;
            }
            catch
            {
                snapshot = [];
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _timer.Tick -= Timer_Tick;
            _timer.Dispose();
        }
    }
}
