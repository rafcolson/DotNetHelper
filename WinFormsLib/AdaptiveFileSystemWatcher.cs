using System.ComponentModel;

namespace WinFormsLib
{
    /// <summary>Provides the directory path associated with a watcher notification.</summary>
    /// <param name="path">The directory path to report.</param>
    public sealed class AdaptiveFileSystemWatcherEventArgs(string path) : EventArgs
    {
        /// <summary>Gets the directory path associated with the notification.</summary>
        public string Path { get; } = path;
    }

    /// <summary>Watches a directory and its immediate contents, using polling when native watching is unavailable or fails to start.</summary>
    /// <remarks>Configure and start the watcher on a Windows Forms UI thread so polling can use its message loop.</remarks>
    public sealed class AdaptiveFileSystemWatcher : IDisposable
    {
        private readonly FileSystemWatcher _folderWatcher = new()
        {
            NotifyFilter = NotifyFilters.DirectoryName
        };
        private readonly FileSystemWatcher _folderContentsWatcher = new()
        {
            NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite
        };
        private readonly PollingFileSystemWatcher _pollingWatcher = new();
        private bool _requiresPolling;
        private bool _disposed;

        /// <summary>Occurs when the watcher detects a change to the directory or its immediate contents.</summary>
        /// <remarks>The reported path identifies a directory. See <see cref="SynchronizingObject"/> for how notifications are dispatched.</remarks>
        public event EventHandler<AdaptiveFileSystemWatcherEventArgs>? Changed;

        /// <summary>Initializes a watcher with event delivery disabled.</summary>
        /// <param name="pollingInterval">The polling interval in milliseconds. Must be greater than zero.</param>
        public AdaptiveFileSystemWatcher(int pollingInterval = 2000)
        {
            _pollingWatcher.Interval = pollingInterval;
            _folderWatcher.Renamed += FileSystemWatcher_Changed;
            _folderWatcher.Deleted += FileSystemWatcher_Changed;
            _folderContentsWatcher.Changed += FileSystemWatcher_Changed;
            _folderContentsWatcher.Renamed += FileSystemWatcher_Changed;
            _folderContentsWatcher.Deleted += FileSystemWatcher_Changed;
            _folderContentsWatcher.Created += FileSystemWatcher_Changed;
            _pollingWatcher.Changed += PollingWatcher_Changed;
        }

        /// <summary>Gets or sets the directory to watch. Changing it restarts monitoring when events are enabled.</summary>
        /// <remarks>An empty path leaves monitoring inactive.</remarks>
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
                _requiresPolling = !string.IsNullOrEmpty(value) && !Utils.SupportsFileSystemWatcher(value);
                if (EnableRaisingEvents)
                {
                    Restart();
                }
            }
        } = string.Empty;

        /// <summary>Gets or sets the polling interval in milliseconds. The value must be greater than zero.</summary>
        public int PollingInterval
        {
            get => _pollingWatcher.Interval;
            set => _pollingWatcher.Interval = value;
        }

        /// <summary>Gets or sets the object used to marshal native file system notifications to its thread.</summary>
        /// <remarks>When this property is <see langword="null"/>, native notifications can arrive on a background thread. Polling notifications are raised on the thread that runs the Windows Forms timer.</remarks>
        public ISynchronizeInvoke? SynchronizingObject
        {
            get => _folderContentsWatcher.SynchronizingObject;
            set
            {
                _folderWatcher.SynchronizingObject = value;
                _folderContentsWatcher.SynchronizingObject = value;
            }
        }

        /// <summary>Gets or sets whether monitoring is enabled for the configured path.</summary>
        public bool EnableRaisingEvents
        {
            get;
            set
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (field == value)
                {
                    return;
                }
                field = value;
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        /// <summary>Gets whether the polling timer is currently running.</summary>
        public bool UsesPolling => _pollingWatcher.EnableRaisingEvents;

        private void Restart()
        {
            Stop();
            Start();
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(Path))
            {
                return;
            }
            if (_requiresPolling)
            {
                StartPolling();
                return;
            }

            try
            {
                _folderContentsWatcher.Path = Path;
                _folderContentsWatcher.EnableRaisingEvents = true;

                string filter = Utils.GetDirectoryName(Path);
                string parentPath = Utils.GetParentPath(Path);
                if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(parentPath))
                {
                    _folderWatcher.Filter = filter;
                    _folderWatcher.Path = parentPath;
                    _folderWatcher.EnableRaisingEvents = true;
                }
            }
            catch
            {
                _folderWatcher.EnableRaisingEvents = false;
                _folderContentsWatcher.EnableRaisingEvents = false;
                StartPolling();
            }
        }

        private void StartPolling()
        {
            _pollingWatcher.Path = Path;
            _pollingWatcher.EnableRaisingEvents = true;
        }

        private void Stop()
        {
            _pollingWatcher.EnableRaisingEvents = false;
            _folderWatcher.EnableRaisingEvents = false;
            _folderContentsWatcher.EnableRaisingEvents = false;
        }

        private void FileSystemWatcher_Changed(object? sender, FileSystemEventArgs e)
        {
            string path = ReferenceEquals(sender, _folderWatcher) ? e.FullPath : Path;
            Changed?.Invoke(this, new AdaptiveFileSystemWatcherEventArgs(path));
        }

        private void PollingWatcher_Changed(object? sender, EventArgs e) => Changed?.Invoke(this, new AdaptiveFileSystemWatcherEventArgs(Path));

        /// <summary>Stops monitoring and releases the underlying watchers and polling timer.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            Stop();
            _folderWatcher.Renamed -= FileSystemWatcher_Changed;
            _folderWatcher.Deleted -= FileSystemWatcher_Changed;
            _folderContentsWatcher.Changed -= FileSystemWatcher_Changed;
            _folderContentsWatcher.Renamed -= FileSystemWatcher_Changed;
            _folderContentsWatcher.Deleted -= FileSystemWatcher_Changed;
            _folderContentsWatcher.Created -= FileSystemWatcher_Changed;
            _pollingWatcher.Changed -= PollingWatcher_Changed;
            _folderWatcher.Dispose();
            _folderContentsWatcher.Dispose();
            _pollingWatcher.Dispose();
        }
    }
}
