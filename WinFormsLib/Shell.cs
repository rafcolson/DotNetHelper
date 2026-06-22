using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WinFormsLib
{
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

        public bool FastSystemPropertiesOnly => Properties.System.FastSystemPropertiesOnly;
        public ShellProperties Properties { get; }
        public ShellThumbnail Thumbnail { get; }

        public static ShellObject FromParsingName(string path, bool fastSystemPropertiesOnly = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new FileNotFoundException("The shell object path does not exist.", path);
            }
            return new ShellObject(path, fastSystemPropertiesOnly);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            Thumbnail.Dispose();
        }

        public override bool Equals(object? obj) => obj is ShellObject so && string.Equals(_path, so._path, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(_path);
    }

    public sealed class ShellProperties
    {
        public ShellProperties(string path, bool fastSystemPropertiesOnly = false)
        {
            System = new SystemProperties(path, fastSystemPropertiesOnly);
            DefaultPropertyCollection = new ShellPropertyCollection(path);
        }

        public SystemProperties System { get; }
        public ShellPropertyCollection DefaultPropertyCollection { get; }
    }

    public sealed class ShellPropertyCollection(string path)
    {
        private readonly string _path = path;

        public ShellProperty<object?> this[string key] => key switch
        {
            "System.FileAttributes" => new ShellProperty<object?>(() => (uint)File.GetAttributes(_path)),
            _ => throw new IndexOutOfRangeException(key),
        };
    }

    public sealed class SystemProperties
    {
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

        public bool FastSystemPropertiesOnly { get; }
        public ShellProperty<int?> PerceivedType { get; }
        public ShellProperty<string> ItemTypeText { get; }
        public ShellProperty<string> ItemType { get; }
        public ShellProperty<DateTime?> DateModified { get; }
        public ShellProperty<DateTime?> DateCreated { get; }
        public ShellProperty<ulong?> Size { get; }

        public ShellProperty<string> Title { get; }
        public ShellProperty<string> Subject { get; }
        public ShellProperty<string> Comment { get; }
        public ShellProperty<string[]> Keywords { get; }

        public PhotoProperties Photo { get; }
        public MediaProperties Media { get; }
        public GpsProperties GPS { get; }

        private GetPropertyStoreFlags GetFlags() => FastSystemPropertiesOnly ? GetPropertyStoreFlags.FastPropertiesOnly : GetPropertyStoreFlags.Default;
    }

    public sealed class PhotoProperties
    {
        public PhotoProperties(string path, bool fastSystemPropertiesOnly = false)
        {
            FastSystemPropertiesOnly = fastSystemPropertiesOnly;
            GetPropertyStoreFlags flags = GetFlags();

            CameraManufacturer = ShellPropertyStore.Create(path, "System.Photo.CameraManufacturer", string.Empty, flags);
            CameraModel = ShellPropertyStore.Create(path, "System.Photo.CameraModel", string.Empty, flags);
            DateTaken = ShellPropertyStore.Create<DateTime?>(path, "System.Photo.DateTaken", null, flags);
            Orientation = ShellPropertyStore.Create<ushort?>(path, "System.Photo.Orientation", null, flags);
        }

        public bool FastSystemPropertiesOnly { get; }
        public ShellProperty<string> CameraManufacturer { get; }
        public ShellProperty<string> CameraModel { get; }
        public ShellProperty<DateTime?> DateTaken { get; }
        public ShellProperty<ushort?> Orientation { get; }

        private GetPropertyStoreFlags GetFlags() => FastSystemPropertiesOnly ? GetPropertyStoreFlags.FastPropertiesOnly : GetPropertyStoreFlags.Default;
    }

    public sealed class MediaProperties
    {
        public MediaProperties(string path, bool fastSystemPropertiesOnly = false)
        {
            FastSystemPropertiesOnly = fastSystemPropertiesOnly;
            DateEncoded = ShellPropertyStore.Create<DateTime?>(path, "System.Media.DateEncoded", null, GetFlags());
        }

        public bool FastSystemPropertiesOnly { get; }
        public ShellProperty<DateTime?> DateEncoded { get; }

        private GetPropertyStoreFlags GetFlags() => FastSystemPropertiesOnly ? GetPropertyStoreFlags.FastPropertiesOnly : GetPropertyStoreFlags.Default;
    }

    public sealed class GpsProperties
    {
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

        public bool FastSystemPropertiesOnly { get; }
        public ShellProperty<string> AreaInformation { get; }
        public ShellProperty<double[]> Latitude { get; }
        public ShellProperty<double[]> Longitude { get; }
        public ShellProperty<string> LatitudeRef { get; }
        public ShellProperty<string> LongitudeRef { get; }
        public ShellProperty<uint[]> LatitudeNumerator { get; }
        public ShellProperty<uint[]> LongitudeNumerator { get; }
        public ShellProperty<uint[]> LatitudeDenominator { get; }
        public ShellProperty<uint[]> LongitudeDenominator { get; }

        private GetPropertyStoreFlags GetFlags() => FastSystemPropertiesOnly ? GetPropertyStoreFlags.FastPropertiesOnly : GetPropertyStoreFlags.Default;
    }

    public sealed class ShellProperty<T>
    {
        private readonly Func<T>? _getter;
        private readonly Action<T>? _setter;
        private T _value = default!;

        public ShellProperty()
        {
        }

        public ShellProperty(T value)
        {
            _value = value;
        }

        public ShellProperty(Func<T> getter, Action<T>? setter = null)
        {
            _getter = getter;
            _setter = setter;
        }

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

        public object? ValueAsObject => Value;
    }

    public sealed class ShellThumbnail(string path) : IDisposable
    {
        private readonly string _path = path;
        private Bitmap? _smallBitmap;
        private Bitmap? _mediumBitmap;
        private Bitmap? _largeBitmap;
        private Bitmap? _extraLargeBitmap;
        private bool _disposed;

        public Bitmap? SmallBitmap => _smallBitmap ??= GetBitmap(16);
        public Bitmap? MediumBitmap => _mediumBitmap ??= GetBitmap(48);
        public Bitmap? LargeBitmap => _largeBitmap ??= GetBitmap(128);
        public Bitmap? ExtraLargeBitmap => _extraLargeBitmap ??= GetBitmap(256);

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

    internal static class ShellImageFactory
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
                SHCreateItemFromParsingName(path, IntPtr.Zero, ref guid, out imageFactory);
                imageFactory.GetImage(new ShellSize(size, size), ShellItemImageFactoryOptions.ThumbnailOnly | ShellItemImageFactoryOptions.BiggerSizeOk, out bitmapHandle);
                if (bitmapHandle == IntPtr.Zero)
                {
                    imageFactory.GetImage(new ShellSize(size, size), ShellItemImageFactoryOptions.IconOnly | ShellItemImageFactoryOptions.BiggerSizeOk, out bitmapHandle);
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
                    DeleteObject(bitmapHandle);
                }
                if (imageFactory != null && Marshal.IsComObject(imageFactory))
                {
                    Marshal.ReleaseComObject(imageFactory);
                }
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            IntPtr pbc,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory ppv);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHChangeNotify(
            uint wEventId,
            uint uFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string dwItem1,
            IntPtr dwItem2);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);
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
            if (string.IsNullOrEmpty(extension))
            {
                return 0;
            }
            if (ImageExtensions.Contains(extension))
            {
                return 2;
            }
            if (AudioExtensions.Contains(extension))
            {
                return 3;
            }
            if (VideoExtensions.Contains(extension))
            {
                return 4;
            }
            if (CompressedExtensions.Contains(extension))
            {
                return 5;
            }
            if (DocumentExtensions.Contains(extension))
            {
                return 6;
            }
            if (string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
            {
                return 8;
            }
            if (string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }
            return 0;
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

    internal static class ShellPropertyStore
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
            SHGetPropertyStoreFromParsingName(path, IntPtr.Zero, flags, ref guid, out IPropertyStore propertyStore);
            return propertyStore;
        }

        private static void Release(object? comObject)
        {
            if (comObject != null && Marshal.IsComObject(comObject))
            {
                Marshal.ReleaseComObject(comObject);
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHGetPropertyStoreFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            IntPtr pbc,
            GetPropertyStoreFlags flags,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IPropertyStore ppv);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode)]
        private static extern int PSGetPropertyKeyFromName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszName,
            out PropertyKey pkey);

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant pvar);
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
            if (value == null)
            {
                return new PropVariant { _valueType = VtEmpty };
            }

            if (value is string stringValue)
            {
                return new PropVariant
                {
                    _valueType = VtLpWStr,
                    _pointerValue = Marshal.StringToCoTaskMemUni(stringValue),
                };
            }

            if (value is string[] stringValues)
            {
                return FromStringVector(stringValues);
            }

            if (value is int intValue)
            {
                return new PropVariant(intValue);
            }

            if (value is uint uintValue)
            {
                return new PropVariant(uintValue);
            }

            if (value is ulong ulongValue)
            {
                return new PropVariant(ulongValue);
            }

            if (value is DateTime dateTimeValue)
            {
                return new PropVariant
                {
                    _valueType = VtFileTime,
                    _fileTimeValue = dateTimeValue.ToUniversalTime().ToFileTimeUtc(),
                };
            }

            if (value is ushort ushortValue)
            {
                return new PropVariant
                {
                    _valueType = VtUi2,
                    _ushortValue = ushortValue,
                };
            }

            if (value is uint[] uintValues)
            {
                return FromUIntVector(uintValues);
            }
            if (value is double[] doubleValues)
            {
                return FromDoubleVector(doubleValues);
            }

            return new PropVariant { _valueType = VtEmpty };
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

            if (value is T typedValue)
            {
                return typedValue;
            }
            if (value != null && Nullable.GetUnderlyingType(typeof(T)) != null)
            {
                return (T)value;
            }
            return defaultValue;
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
