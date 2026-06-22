using System.ComponentModel;

namespace WinFormsLib
{
    public sealed class AdaptiveFileSystemWatcherEventArgs(string path) : EventArgs
    {
        public string Path { get; } = path;
    }

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
        private string _path = string.Empty;
        private bool _requiresPolling;
        private bool _enableRaisingEvents;
        private bool _disposed;

        public event EventHandler<AdaptiveFileSystemWatcherEventArgs>? Changed;

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
                _requiresPolling = !string.IsNullOrEmpty(value) && !Utils.SupportsFileSystemWatcher(value);
                if (_enableRaisingEvents)
                {
                    Restart();
                }
            }
        }

        public int PollingInterval
        {
            get => _pollingWatcher.Interval;
            set => _pollingWatcher.Interval = value;
        }

        public ISynchronizeInvoke? SynchronizingObject
        {
            get => _folderContentsWatcher.SynchronizingObject;
            set
            {
                _folderWatcher.SynchronizingObject = value;
                _folderContentsWatcher.SynchronizingObject = value;
            }
        }

        public bool EnableRaisingEvents
        {
            get => _enableRaisingEvents;
            set
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (_enableRaisingEvents == value)
                {
                    return;
                }
                _enableRaisingEvents = value;
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

        public bool UsesPolling => _pollingWatcher.EnableRaisingEvents;

        private void Restart()
        {
            Stop();
            Start();
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(_path))
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
                _folderContentsWatcher.Path = _path;
                _folderContentsWatcher.EnableRaisingEvents = true;

                string filter = Utils.GetDirectoryName(_path);
                string parentPath = Utils.GetParentPath(_path);
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
            _pollingWatcher.Path = _path;
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
            string path = ReferenceEquals(sender, _folderWatcher) ? e.FullPath : _path;
            Changed?.Invoke(this, new AdaptiveFileSystemWatcherEventArgs(path));
        }

        private void PollingWatcher_Changed(object? sender, EventArgs e) => Changed?.Invoke(this, new AdaptiveFileSystemWatcherEventArgs(_path));

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
