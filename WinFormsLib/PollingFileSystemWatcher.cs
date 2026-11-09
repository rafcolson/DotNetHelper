namespace WinFormsLib
{
    /// <summary>Detects changes to a directory by periodically comparing snapshots of its immediate contents.</summary>
    /// <remarks>Configure and start the watcher on a Windows Forms UI thread with a message loop.</remarks>
    public sealed class PollingFileSystemWatcher : IDisposable
    {
        private readonly record struct FileSystemEntryState(string Path, FileAttributes Attributes, long Length, long LastWriteTimeUtcTicks);

        private readonly System.Windows.Forms.Timer _timer = new();
        private FileSystemEntryState[] _snapshot = [];
        private bool _directoryExists;
        private bool _initialized;
        private bool _disposed;

        /// <summary>Occurs when a poll detects a change in directory existence or in the names, attributes, sizes, or last write times of its entries.</summary>
        /// <remarks>The event is raised on the thread that runs the Windows Forms timer.</remarks>
        public event EventHandler? Changed;

        /// <summary>Initializes a watcher with polling disabled.</summary>
        public PollingFileSystemWatcher()
        {
            _timer.Tick += Timer_Tick;
        }

        /// <summary>Gets or sets the directory to poll. Changing it while polling establishes a new baseline.</summary>
        public string Path
        {
            get;
            set
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (string.Equals(field, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                field = value;
                if (EnableRaisingEvents)
                {
                    InitializeSnapshot();
                }
            }
        } = string.Empty;

        /// <summary>Gets or sets the polling interval in milliseconds. The value must be greater than zero.</summary>
        public int Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        /// <summary>Gets or sets whether polling is active. Setting it to <see langword="true"/> takes an initial snapshot before starting the timer.</summary>
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
            _initialized = TryGetSnapshot(Path, out _directoryExists, out _snapshot);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!TryGetSnapshot(Path, out bool directoryExists, out FileSystemEntryState[] snapshot))
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

        /// <summary>Stops polling and releases the timer.</summary>
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
