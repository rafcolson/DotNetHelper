using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WinFormsLib
{
    /// <summary>Represents a file or directory and exposes its Windows Shell metadata and thumbnails.</summary>
    public sealed class ShellObject : IDisposable
    {
        private readonly string _path;
        private bool _disposed;

        private ShellObject(string path, bool fastSystemPropertiesOnly)
        {
            _path = path;
            Properties = new ShellProperties(path, fastSystemPropertiesOnly);
            Thumbnail = new ShellThumbnail(path);
        }

        /// <summary>Gets whether metadata reads request only fast Shell properties.</summary>
        public bool FastSystemPropertiesOnly => Properties.System.FastSystemPropertiesOnly;
        /// <summary>Gets the metadata groups for this Shell object.</summary>
        public ShellProperties Properties { get; }
        /// <summary>Gets the thumbnail cache owned by this Shell object.</summary>
        public ShellThumbnail Thumbnail { get; }

        /// <summary>Creates a Shell object for an existing file or directory.</summary>
        /// <param name="path">The file or directory path.</param>
        /// <param name="fastSystemPropertiesOnly">Whether to request only fast Shell properties when reading metadata.</param>
        /// <returns>A Shell object that the caller must dispose.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is null, empty, or whitespace.</exception>
        /// <exception cref="FileNotFoundException">The path does not identify an existing file or directory.</exception>
        public static ShellObject FromParsingName(string path, bool fastSystemPropertiesOnly = false)
        {
            return string.IsNullOrWhiteSpace(path)
                ? throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path))
                : !File.Exists(path) && !Directory.Exists(path)
                ? throw new FileNotFoundException("The shell object path does not exist.", path)
                : new ShellObject(path, fastSystemPropertiesOnly);
        }

        /// <summary>Disposes the thumbnail cache owned by this Shell object.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            Thumbnail.Dispose();
        }

        /// <summary>Compares Shell objects by their stored paths, using an ordinal case-insensitive comparison.</summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the other object is a Shell object with a matching path.</returns>
        public override bool Equals(object? obj) => obj is ShellObject so && string.Equals(_path, so._path, StringComparison.OrdinalIgnoreCase);

        /// <summary>Returns the ordinal case-insensitive hash code of the stored path.</summary>
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(_path);
    }

    /// <summary>Groups the Shell properties for a file or directory.</summary>
    /// <param name="path">The file or directory path.</param>
    /// <param name="fastSystemPropertiesOnly">Whether to request only fast Shell properties when reading metadata.</param>
    public sealed class ShellProperties(string path, bool fastSystemPropertiesOnly = false)
    {
        /// <summary>Gets system metadata and its specialized property groups.</summary>
        public SystemProperties System { get; } = new SystemProperties(path, fastSystemPropertiesOnly);
        /// <summary>Gets the collection of properties accessible by name.</summary>
        public ShellPropertyCollection DefaultPropertyCollection { get; } = new ShellPropertyCollection(path);
    }

    /// <summary>Provides access to supported Shell properties by name.</summary>
    /// <param name="path">The file or directory path.</param>
    public sealed class ShellPropertyCollection(string path)
    {
        private readonly string _path = path;

        /// <summary>Gets the named property. The supported key is <c>System.FileAttributes</c>.</summary>
        /// <param name="key">The property name.</param>
        /// <exception cref="IndexOutOfRangeException">The property name is unsupported.</exception>
        public ShellProperty<object?> this[string key] => key switch
        {
            "System.FileAttributes" => new ShellProperty<object?>(() => (uint)File.GetAttributes(_path)),
            _ => throw new IndexOutOfRangeException(key),
        };
    }

    /// <summary>Exposes system metadata for a file or directory.</summary>
    public sealed class SystemProperties
    {
        /// <summary>Initializes the metadata group for the specified path.</summary>
        /// <param name="path">The file or directory path.</param>
        /// <param name="fastSystemPropertiesOnly">Whether to request only fast Shell properties when reading metadata.</param>
        public SystemProperties(string path, bool fastSystemPropertiesOnly = false)
        {
            FastSystemPropertiesOnly = fastSystemPropertiesOnly;
            GetPropertyStoreFlags flags = GetFlags();

            PerceivedType = ShellPropertyStore.Create(path, "System.PerceivedType", ShellPropertyReader.GetPerceivedType(path), flags);
            ItemTypeText = ShellPropertyStore.Create(path, "System.ItemTypeText", ShellPropertyReader.GetItemTypeText(path), flags);
            ItemType = ShellPropertyStore.Create(path, "System.ItemType", Path.GetExtension(path), flags);
            DateModified = new ShellProperty<DateTime?>(() => File.Exists(path) || Directory.Exists(path) ? File.GetLastWriteTime(path) : null);
            DateCreated = new ShellProperty<DateTime?>(() => File.Exists(path) || Directory.Exists(path) ? File.GetCreationTime(path) : null);
            Size = ShellPropertyStore.Create<ulong?>(path, "System.Size", File.Exists(path) ? (ulong)new FileInfo(path).Length : null, flags);

            Title = ShellPropertyStore.Create(path, "System.Title", string.Empty, flags);
            Subject = ShellPropertyStore.Create(path, "System.Subject", string.Empty, flags);
            Comment = ShellPropertyStore.Create(path, "System.Comment", string.Empty, flags);
            Keywords = ShellPropertyStore.Create(path, "System.Keywords", Array.Empty<string>(), flags);

            Photo = new PhotoProperties(path, FastSystemPropertiesOnly);
            Media = new MediaProperties(path, FastSystemPropertiesOnly);
            GPS = new GpsProperties(path, FastSystemPropertiesOnly);
        }

        /// <summary>Gets whether metadata reads request only fast Shell properties.</summary>
        public bool FastSystemPropertiesOnly { get; }
        /// <summary>Gets the property for the Shell's numeric classification of the item.</summary>
        public ShellProperty<int?> PerceivedType { get; }
        /// <summary>Gets the property for the display text of the item type.</summary>
        public ShellProperty<string> ItemTypeText { get; }
        /// <summary>Gets the property for the Shell item type identifier.</summary>
        public ShellProperty<string> ItemType { get; }
        /// <summary>Gets the local last-write time property.</summary>
        public ShellProperty<DateTime?> DateModified { get; }
        /// <summary>Gets the local creation time property.</summary>
        public ShellProperty<DateTime?> DateCreated { get; }
        /// <summary>Gets the item size property in bytes.</summary>
        public ShellProperty<ulong?> Size { get; }

        /// <summary>Gets the item's title metadata property.</summary>
        public ShellProperty<string> Title { get; }
        /// <summary>Gets the item's subject metadata property.</summary>
        public ShellProperty<string> Subject { get; }
        /// <summary>Gets the item's comment metadata property.</summary>
        public ShellProperty<string> Comment { get; }
        /// <summary>Gets the item's keyword metadata property.</summary>
        public ShellProperty<string[]> Keywords { get; }

        /// <summary>Gets the photo metadata group.</summary>
        public PhotoProperties Photo { get; }
        /// <summary>Gets the media metadata group.</summary>
        public MediaProperties Media { get; }
        /// <summary>Gets the GPS metadata group.</summary>
        public GpsProperties GPS { get; }

        private GetPropertyStoreFlags GetFlags() => FastSystemPropertiesOnly ? GetPropertyStoreFlags.FastPropertiesOnly : GetPropertyStoreFlags.Default;
    }

    /// <summary>Exposes photo metadata reported by the Windows Shell.</summary>
    public sealed class PhotoProperties
    {
        /// <summary>Initializes the metadata group for the specified path.</summary>
        /// <param name="path">The file or directory path.</param>
        /// <param name="fastSystemPropertiesOnly">Whether to request only fast Shell properties when reading metadata.</param>
        public PhotoProperties(string path, bool fastSystemPropertiesOnly = false)
        {
            FastSystemPropertiesOnly = fastSystemPropertiesOnly;
            GetPropertyStoreFlags flags = GetFlags();

            CameraManufacturer = ShellPropertyStore.Create(path, "System.Photo.CameraManufacturer", string.Empty, flags);
            CameraModel = ShellPropertyStore.Create(path, "System.Photo.CameraModel", string.Empty, flags);
            DateTaken = ShellPropertyStore.Create<DateTime?>(path, "System.Photo.DateTaken", null, flags);
            Orientation = ShellPropertyStore.Create<ushort?>(path, "System.Photo.Orientation", null, flags);
        }

        /// <summary>Gets whether metadata reads request only fast Shell properties.</summary>
        public bool FastSystemPropertiesOnly { get; }
        /// <summary>Gets the camera manufacturer property.</summary>
        public ShellProperty<string> CameraManufacturer { get; }
        /// <summary>Gets the camera model property.</summary>
        public ShellProperty<string> CameraModel { get; }
        /// <summary>Gets the capture date and time property.</summary>
        public ShellProperty<DateTime?> DateTaken { get; }
        /// <summary>Gets the recorded photo orientation property.</summary>
        public ShellProperty<ushort?> Orientation { get; }

        private GetPropertyStoreFlags GetFlags() => FastSystemPropertiesOnly ? GetPropertyStoreFlags.FastPropertiesOnly : GetPropertyStoreFlags.Default;
    }

    /// <summary>Exposes media metadata reported by the Windows Shell.</summary>
    public sealed class MediaProperties
    {
        /// <summary>Initializes the metadata group for the specified path.</summary>
        /// <param name="path">The file or directory path.</param>
        /// <param name="fastSystemPropertiesOnly">Whether to request only fast Shell properties when reading metadata.</param>
        public MediaProperties(string path, bool fastSystemPropertiesOnly = false)
        {
            FastSystemPropertiesOnly = fastSystemPropertiesOnly;
            DateEncoded = ShellPropertyStore.Create<DateTime?>(path, "System.Media.DateEncoded", null, GetFlags());
        }

        /// <summary>Gets whether metadata reads request only fast Shell properties.</summary>
        public bool FastSystemPropertiesOnly { get; }
        /// <summary>Gets the media encoding date and time property.</summary>
        public ShellProperty<DateTime?> DateEncoded { get; }

        private GetPropertyStoreFlags GetFlags() => FastSystemPropertiesOnly ? GetPropertyStoreFlags.FastPropertiesOnly : GetPropertyStoreFlags.Default;
    }

    /// <summary>Exposes GPS metadata reported by the Windows Shell.</summary>
    public sealed class GpsProperties
    {
        /// <summary>Initializes the metadata group for the specified path.</summary>
        /// <param name="path">The file or directory path.</param>
        /// <param name="fastSystemPropertiesOnly">Whether to request only fast Shell properties when reading metadata.</param>
        public GpsProperties(string path, bool fastSystemPropertiesOnly = false)
        {
            FastSystemPropertiesOnly = fastSystemPropertiesOnly;
            GetPropertyStoreFlags flags = GetFlags();

            AreaInformation = ShellPropertyStore.Create(path, "System.GPS.AreaInformation", string.Empty, flags);
            Latitude = ShellPropertyStore.Create(path, "System.GPS.Latitude", Array.Empty<double>(), flags);
            Longitude = ShellPropertyStore.Create(path, "System.GPS.Longitude", Array.Empty<double>(), flags);
            LatitudeRef = ShellPropertyStore.Create(path, "System.GPS.LatitudeRef", string.Empty, flags);
            LongitudeRef = ShellPropertyStore.Create(path, "System.GPS.LongitudeRef", string.Empty, flags);
            LatitudeNumerator = ShellPropertyStore.Create(path, "System.GPS.LatitudeNumerator", Array.Empty<uint>(), flags);
            LongitudeNumerator = ShellPropertyStore.Create(path, "System.GPS.LongitudeNumerator", Array.Empty<uint>(), flags);
            LatitudeDenominator = ShellPropertyStore.Create(path, "System.GPS.LatitudeDenominator", Array.Empty<uint>(), flags);
            LongitudeDenominator = ShellPropertyStore.Create(path, "System.GPS.LongitudeDenominator", Array.Empty<uint>(), flags);
        }

        /// <summary>Gets whether metadata reads request only fast Shell properties.</summary>
        public bool FastSystemPropertiesOnly { get; }
        /// <summary>Gets the recorded GPS area information property.</summary>
        public ShellProperty<string> AreaInformation { get; }
        /// <summary>Gets the latitude components reported by the Shell.</summary>
        public ShellProperty<double[]> Latitude { get; }
        /// <summary>Gets the longitude components reported by the Shell.</summary>
        public ShellProperty<double[]> Longitude { get; }
        /// <summary>Gets the latitude reference, typically north or south.</summary>
        public ShellProperty<string> LatitudeRef { get; }
        /// <summary>Gets the longitude reference, typically east or west.</summary>
        public ShellProperty<string> LongitudeRef { get; }
        /// <summary>Gets the numerators of the latitude components.</summary>
        public ShellProperty<uint[]> LatitudeNumerator { get; }
        /// <summary>Gets the numerators of the longitude components.</summary>
        public ShellProperty<uint[]> LongitudeNumerator { get; }
        /// <summary>Gets the denominators of the latitude components.</summary>
        public ShellProperty<uint[]> LatitudeDenominator { get; }
        /// <summary>Gets the denominators of the longitude components.</summary>
        public ShellProperty<uint[]> LongitudeDenominator { get; }

        private GetPropertyStoreFlags GetFlags() => FastSystemPropertiesOnly ? GetPropertyStoreFlags.FastPropertiesOnly : GetPropertyStoreFlags.Default;
    }

    /// <summary>Represents a stored value or a value accessed through delegates.</summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    public sealed class ShellProperty<T>
    {
        private readonly Func<T>? _getter;
        private readonly Action<T>? _setter;
        private T _value = default!;

        /// <summary>Initializes a stored property with the default value of its type.</summary>
        public ShellProperty()
        {
        }

        /// <summary>Initializes a stored property with the supplied value.</summary>
        /// <param name="value">The initial value.</param>
        public ShellProperty(T value)
        {
            _value = value;
        }

        /// <summary>Initializes a property backed by delegates.</summary>
        /// <param name="getter">The delegate invoked on every read.</param>
        /// <param name="setter">The delegate invoked on writes, if supplied.</param>
        /// <remarks>With a getter and no setter, assigning a value does not affect subsequent reads.</remarks>
        public ShellProperty(Func<T> getter, Action<T>? setter = null)
        {
            _getter = getter;
            _setter = setter;
        }

        /// <summary>Gets or sets the value, invoking the configured delegate for the operation when present.</summary>
        public T Value
        {
            get => _getter == null ? _value : _getter();
            set
            {
                if (_setter == null)
                {
                    _value = value;
                }
                else
                {
                    _setter(value);
                }
            }
        }

        /// <summary>Gets the current value as an object, boxing value types.</summary>
        public object? ValueAsObject => Value;
    }

    /// <summary>Loads and caches thumbnails or icons supplied by the Windows Shell.</summary>
    /// <param name="path">The file or directory path.</param>
    /// <remarks>Returned bitmaps belong to the cache. Clone them if they must outlive a refresh or disposal.</remarks>
    public sealed class ShellThumbnail(string path) : IDisposable
    {
        private readonly string _path = path;
        private Bitmap? _smallBitmap;
        private Bitmap? _mediumBitmap;
        private Bitmap? _largeBitmap;
        private Bitmap? _extraLargeBitmap;
        private bool _disposed;

        /// <summary>Gets the image requested at 16 pixels, or <see langword="null"/> if the Shell cannot supply one.</summary>
        public Bitmap? SmallBitmap => _smallBitmap ??= GetBitmap(16);
        /// <summary>Gets the image requested at 48 pixels, or <see langword="null"/> if the Shell cannot supply one.</summary>
        public Bitmap? MediumBitmap => _mediumBitmap ??= GetBitmap(48);
        /// <summary>Gets the image requested at 128 pixels, or <see langword="null"/> if the Shell cannot supply one.</summary>
        public Bitmap? LargeBitmap => _largeBitmap ??= GetBitmap(128);
        /// <summary>Gets the image requested at 256 pixels, or <see langword="null"/> if the Shell cannot supply one.</summary>
        public Bitmap? ExtraLargeBitmap => _extraLargeBitmap ??= GetBitmap(256);

        /// <summary>Disposes cached bitmaps and notifies the Shell that the item has changed.</summary>
        /// <exception cref="ObjectDisposedException">The thumbnail cache has been disposed.</exception>
        public void Refresh()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _smallBitmap?.Dispose();
            _mediumBitmap?.Dispose();
            _largeBitmap?.Dispose();
            _extraLargeBitmap?.Dispose();
            _smallBitmap = null;
            _mediumBitmap = null;
            _largeBitmap = null;
            _extraLargeBitmap = null;
            ShellImageFactory.NotifyUpdate(_path);
        }

        /// <summary>Disposes the cached bitmaps.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _smallBitmap?.Dispose();
            _mediumBitmap?.Dispose();
            _largeBitmap?.Dispose();
            _extraLargeBitmap?.Dispose();
        }

        private Bitmap? GetBitmap(int size)
        {
            try
            {
                return ShellImageFactory.GetBitmap(_path, size);
            }
            catch
            {
            }

            return null;
        }
    }

    internal static partial class ShellImageFactory
    {
        private const uint SHCNE_UPDATEITEM = 0x00002000;
        private const uint SHCNF_PATHW = 0x0005;

        public static void NotifyUpdate(string path) => SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATHW, path, IntPtr.Zero);

        public static Bitmap? GetBitmap(string path, int size)
        {
            IShellItemImageFactory? imageFactory = null;
            IntPtr bitmapHandle = IntPtr.Zero;

            try
            {
                Guid guid = typeof(IShellItemImageFactory).GUID;
                int result = SHCreateItemFromParsingName(path, IntPtr.Zero, ref guid, out IntPtr imageFactoryPointer);
                try
                {
                    Marshal.ThrowExceptionForHR(result);
                    imageFactory = (IShellItemImageFactory)Marshal.GetObjectForIUnknown(imageFactoryPointer);
                }
                finally
                {
                    if (imageFactoryPointer != IntPtr.Zero)
                    {
                        _ = Marshal.Release(imageFactoryPointer);
                    }
                }
                _ = imageFactory.GetImage(new ShellSize(size, size), ShellItemImageFactoryOptions.ThumbnailOnly | ShellItemImageFactoryOptions.BiggerSizeOk, out bitmapHandle);
                if (bitmapHandle == IntPtr.Zero)
                {
                    _ = imageFactory.GetImage(new ShellSize(size, size), ShellItemImageFactoryOptions.IconOnly | ShellItemImageFactoryOptions.BiggerSizeOk, out bitmapHandle);
                }
                return bitmapHandle == IntPtr.Zero ? null : Image.FromHbitmap(bitmapHandle);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (bitmapHandle != IntPtr.Zero)
                {
                    _ = DeleteObject(bitmapHandle);
                }
                if (imageFactory != null && Marshal.IsComObject(imageFactory))
                {
                    _ = Marshal.ReleaseComObject(imageFactory);
                }
            }
        }

        [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SHCreateItemFromParsingName(
            string pszPath,
            IntPtr pbc,
            ref Guid riid,
            out IntPtr ppv);

        [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial void SHChangeNotify(
            uint wEventId,
            uint uFlags,
            string dwItem1,
            IntPtr dwItem2);

        [LibraryImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DeleteObject(IntPtr hObject);
    }

    [ComImport]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellItemImageFactory
    {
        [PreserveSig]
        int GetImage(ShellSize size, ShellItemImageFactoryOptions flags, out IntPtr phbm);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct ShellSize(int width, int height)
    {
        private readonly int _width = width;
        private readonly int _height = height;
    }

    [Flags]
    internal enum ShellItemImageFactoryOptions
    {
        BiggerSizeOk = 1,
        IconOnly = 4,
        ThumbnailOnly = 8,
    }

    internal static class ShellPropertyReader
    {
        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".bmp", ".gif", ".heic", ".jpeg", ".jpg", ".png", ".tif", ".tiff", ".webp",
        };

        private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".aac", ".flac", ".m4a", ".mp3", ".ogg", ".wav", ".wma",
        };

        private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".avi", ".m4v", ".mkv", ".mov", ".mp4", ".mpeg", ".mpg", ".webm", ".wmv",
        };

        private static readonly HashSet<string> CompressedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".7z", ".cab", ".gz", ".rar", ".tar", ".zip",
        };

        private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".doc", ".docx", ".odt", ".pdf", ".ppt", ".pptx", ".rtf", ".xls", ".xlsx",
        };

        public static bool IsImage(string path) => ImageExtensions.Contains(Path.GetExtension(path));

        public static int? GetPerceivedType(string path)
        {
            string extension = Path.GetExtension(path);
            return string.IsNullOrEmpty(extension)
                ? 0
                : ImageExtensions.Contains(extension)
                ? 2
                : AudioExtensions.Contains(extension)
                ? 3
                : VideoExtensions.Contains(extension)
                ? 4
                : CompressedExtensions.Contains(extension)
                ? 5
                : DocumentExtensions.Contains(extension)
                ? 6
                : string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase)
                ? 8
                : string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        }

        public static string GetItemTypeText(string path)
        {
            string extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
            {
                return Directory.Exists(path) ? "File folder" : string.Empty;
            }

            try
            {
                using RegistryKey? extensionKey = Registry.ClassesRoot.OpenSubKey(extension);
                if (extensionKey?.GetValue(null) is string progId)
                {
                    using RegistryKey? progIdKey = Registry.ClassesRoot.OpenSubKey(progId);
                    if (progIdKey?.GetValue(null) is string description && !string.IsNullOrWhiteSpace(description))
                    {
                        return description;
                    }
                }
            }
            catch
            {
            }

            return $"{extension.TrimStart('.').ToUpperInvariant()} File";
        }
    }

    internal static partial class ShellPropertyStore
    {
        public static ShellProperty<T> Create<T>(string path, string canonicalName, T defaultValue, GetPropertyStoreFlags flags = GetPropertyStoreFlags.Default)
        {
            return new ShellProperty<T>(
                () => GetValue(path, canonicalName, defaultValue, flags),
                value => SetValue(path, canonicalName, value));
        }

        private static T GetValue<T>(string path, string canonicalName, T defaultValue, GetPropertyStoreFlags flags)
        {
            if (!TryGetPropertyKey(canonicalName, out PropertyKey propertyKey))
            {
                return defaultValue;
            }

            IPropertyStore? propertyStore = null;
            PropVariant propVariant = default;
            try
            {
                propertyStore = GetPropertyStore(path, flags);
                return propertyStore.GetValue(ref propertyKey, out propVariant) == 0
                    ? propVariant.ToValue(defaultValue)
                    : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
            finally
            {
                _ = PropVariantClear(ref propVariant);
                Release(propertyStore);
            }
        }

        private static void SetValue<T>(string path, string canonicalName, T value)
        {
            if (!TryGetPropertyKey(canonicalName, out PropertyKey propertyKey))
            {
                return;
            }

            IPropertyStore? propertyStore = null;
            PropVariant propVariant = PropVariant.FromValue(value);
            try
            {
                propertyStore = GetPropertyStore(path, GetPropertyStoreFlags.ReadWrite);
                if (propertyStore.SetValue(ref propertyKey, ref propVariant) == 0)
                {
                    _ = propertyStore.Commit();
                }
            }
            catch
            {
            }
            finally
            {
                _ = PropVariantClear(ref propVariant);
                Release(propertyStore);
            }
        }

        private static bool TryGetPropertyKey(string canonicalName, out PropertyKey propertyKey)
        {
            return PSGetPropertyKeyFromName(canonicalName, out propertyKey) == 0;
        }

        private static IPropertyStore GetPropertyStore(string path, GetPropertyStoreFlags flags)
        {
            Guid guid = typeof(IPropertyStore).GUID;
            int result = SHGetPropertyStoreFromParsingName(path, IntPtr.Zero, flags, ref guid, out IntPtr propertyStorePointer);
            try
            {
                Marshal.ThrowExceptionForHR(result);
                return (IPropertyStore)Marshal.GetObjectForIUnknown(propertyStorePointer);
            }
            finally
            {
                if (propertyStorePointer != IntPtr.Zero)
                {
                    _ = Marshal.Release(propertyStorePointer);
                }
            }
        }

        private static void Release(object? comObject)
        {
            if (comObject != null && Marshal.IsComObject(comObject))
            {
                _ = Marshal.ReleaseComObject(comObject);
            }
        }

        [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SHGetPropertyStoreFromParsingName(
            string pszPath,
            IntPtr pbc,
            GetPropertyStoreFlags flags,
            ref Guid riid,
            out IntPtr ppv);

        [LibraryImport("propsys.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int PSGetPropertyKeyFromName(
            string pszName,
            out PropertyKey pkey);

        [LibraryImport("ole32.dll")]
        private static partial int PropVariantClear(ref PropVariant pvar);
    }

    [Flags]
    internal enum GetPropertyStoreFlags
    {
        Default = 0,
        ReadWrite = 2,
        FastPropertiesOnly = 8,
    }

    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(out uint cProps);

        [PreserveSig]
        int GetAt(uint iProp, out PropertyKey pkey);

        [PreserveSig]
        int GetValue(ref PropertyKey key, out PropVariant pv);

        [PreserveSig]
        int SetValue(ref PropertyKey key, ref PropVariant propvar);

        [PreserveSig]
        int Commit();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct PropertyKey
    {
        public Guid FormatId;
        public uint PropertyId;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct PropVariant
    {
        private const ushort VtEmpty = 0;
        private const ushort VtI4 = 3;
        private const ushort VtUi2 = 18;
        private const ushort VtUi4 = 19;
        private const ushort VtUi8 = 21;
        private const ushort VtFileTime = 64;
        private const ushort VtLpWStr = 31;
        private const ushort VtVector = 0x1000;
        private const ushort VtR8 = 5;

        [FieldOffset(0)]
        private ushort _valueType;

        [FieldOffset(8)]
        private readonly int _intValue;

        [FieldOffset(8)]
        private ushort _ushortValue;

        [FieldOffset(8)]
        private readonly uint _uintValue;

        [FieldOffset(8)]
        private readonly ulong _ulongValue;

        [FieldOffset(8)]
        private long _fileTimeValue;

        [FieldOffset(8)]
        private IntPtr _pointerValue;

        [FieldOffset(8)]
        private uint _vectorCount;

        [FieldOffset(16)]
        private IntPtr _vectorPointer;

        public static PropVariant FromValue<T>(T value)
        {
            return value == null
                ? new PropVariant { _valueType = VtEmpty }
                : value is string stringValue
                ? new PropVariant
                {
                    _valueType = VtLpWStr,
                    _pointerValue = Marshal.StringToCoTaskMemUni(stringValue),
                }
                : value is string[] stringValues
                ? FromStringVector(stringValues)
                : value is int intValue
                ? new PropVariant(intValue)
                : value is uint uintValue
                ? new PropVariant(uintValue)
                : value is ulong ulongValue
                ? new PropVariant(ulongValue)
                : value is DateTime dateTimeValue
                ? new PropVariant
                {
                    _valueType = VtFileTime,
                    _fileTimeValue = dateTimeValue.ToUniversalTime().ToFileTimeUtc(),
                }
                : value is ushort ushortValue
                ? new PropVariant
                {
                    _valueType = VtUi2,
                    _ushortValue = ushortValue,
                }
                : value is uint[] uintValues
                ? FromUIntVector(uintValues)
                : value is double[] doubleValues ? FromDoubleVector(doubleValues) : new PropVariant { _valueType = VtEmpty };
        }

        private PropVariant(int value)
        {
            this = default;
            _valueType = VtI4;
            _intValue = value;
        }

        private PropVariant(uint value)
        {
            this = default;
            _valueType = VtUi4;
            _uintValue = value;
        }

        private PropVariant(ulong value)
        {
            this = default;
            _valueType = VtUi8;
            _ulongValue = value;
        }

        public readonly T ToValue<T>(T defaultValue)
        {
            object? value = null;
            Type type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (type == typeof(string) && _valueType == VtLpWStr && _pointerValue != IntPtr.Zero)
            {
                value = Marshal.PtrToStringUni(_pointerValue);
            }
            else if (type == typeof(string[]) && _valueType == (VtVector | VtLpWStr))
            {
                value = ToStringVector();
            }
            else if (type == typeof(DateTime) && _valueType == VtFileTime)
            {
                value = DateTime.FromFileTimeUtc(_fileTimeValue).ToLocalTime();
            }
            else if (type == typeof(ushort) && _valueType == VtUi2)
            {
                value = _ushortValue;
            }
            else if (type == typeof(int) && _valueType == VtI4)
            {
                value = _intValue;
            }
            else if (type == typeof(int) && _valueType == VtUi4)
            {
                value = unchecked((int)_uintValue);
            }
            else if (type == typeof(uint) && _valueType == VtUi4)
            {
                value = _uintValue;
            }
            else if (type == typeof(uint) && _valueType == VtI4)
            {
                value = unchecked((uint)_intValue);
            }
            else if (type == typeof(ulong) && _valueType == VtUi8)
            {
                value = _ulongValue;
            }
            else if (type == typeof(ulong) && _valueType == VtUi4)
            {
                value = (ulong)_uintValue;
            }
            else if (type == typeof(double[]) && _valueType == (VtVector | VtR8))
            {
                value = ToDoubleVector();
            }
            else if (type == typeof(uint[]) && _valueType == (VtVector | VtUi4))
            {
                value = ToUIntVector();
            }

            return value is T typedValue ? typedValue : value != null && Nullable.GetUnderlyingType(typeof(T)) != null ? (T)value : defaultValue;
        }

        private static PropVariant FromStringVector(string[] values)
        {
            IntPtr vector = Marshal.AllocCoTaskMem(IntPtr.Size * values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.WriteIntPtr(vector, i * IntPtr.Size, Marshal.StringToCoTaskMemUni(values[i]));
            }
            return new PropVariant
            {
                _valueType = VtVector | VtLpWStr,
                _vectorCount = (uint)values.Length,
                _vectorPointer = vector,
            };
        }

        private static PropVariant FromUIntVector(uint[] values)
        {
            IntPtr vector = Marshal.AllocCoTaskMem(sizeof(uint) * values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.WriteInt32(vector, i * sizeof(uint), unchecked((int)values[i]));
            }
            return new PropVariant
            {
                _valueType = VtVector | VtUi4,
                _vectorCount = (uint)values.Length,
                _vectorPointer = vector,
            };
        }

        private static PropVariant FromDoubleVector(double[] values)
        {
            IntPtr vector = Marshal.AllocCoTaskMem(sizeof(double) * values.Length);
            Marshal.Copy(values, 0, vector, values.Length);
            return new PropVariant
            {
                _valueType = VtVector | VtR8,
                _vectorCount = (uint)values.Length,
                _vectorPointer = vector,
            };
        }

        private readonly string[] ToStringVector()
        {
            string[] values = new string[_vectorCount];
            for (int i = 0; i < values.Length; i++)
            {
                IntPtr valuePointer = Marshal.ReadIntPtr(_vectorPointer, i * IntPtr.Size);
                values[i] = Marshal.PtrToStringUni(valuePointer) ?? string.Empty;
            }
            return values;
        }

        private readonly double[] ToDoubleVector()
        {
            double[] values = new double[_vectorCount];
            Marshal.Copy(_vectorPointer, values, 0, values.Length);
            return values;
        }

        private readonly uint[] ToUIntVector()
        {
            uint[] values = new uint[_vectorCount];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = unchecked((uint)Marshal.ReadInt32(_vectorPointer, i * sizeof(uint)));
            }
            return values;
        }
    }
}
