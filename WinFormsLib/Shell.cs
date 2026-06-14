using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WinFormsLib
{
    public sealed class ShellObject : IDisposable
    {
        private readonly string _path;
        private bool _disposed;

        private ShellObject(string path)
        {
            _path = path;
            Properties = new ShellProperties(path);
            Thumbnail = new ShellThumbnail(path);
        }

        public ShellProperties Properties { get; }
        public ShellThumbnail Thumbnail { get; }

        public static ShellObject FromParsingName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            }
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new FileNotFoundException("The shell object path does not exist.", path);
            }
            return new ShellObject(path);
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

    public sealed class ShellProperties(string path)
    {
        public SystemProperties System { get; } = new SystemProperties(path);
        public ShellPropertyCollection DefaultPropertyCollection { get; } = new ShellPropertyCollection(path);
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

    public sealed class SystemProperties(string path)
    {
        public ShellProperty<int?> PerceivedType { get; } = ShellPropertyStore.Create(path, "System.PerceivedType", ShellPropertyReader.GetPerceivedType(path));
        public ShellProperty<string> ItemTypeText { get; } = ShellPropertyStore.Create(path, "System.ItemTypeText", ShellPropertyReader.GetItemTypeText(path));
        public ShellProperty<string> ItemType { get; } = ShellPropertyStore.Create(path, "System.ItemType", Path.GetExtension(path));
        public ShellProperty<DateTime?> DateModified { get; } = new ShellProperty<DateTime?>(() => File.Exists(path) || Directory.Exists(path) ? File.GetLastWriteTime(path) : null);
        public ShellProperty<DateTime?> DateCreated { get; } = new ShellProperty<DateTime?>(() => File.Exists(path) || Directory.Exists(path) ? File.GetCreationTime(path) : null);
        public ShellProperty<ulong?> Size { get; } = new ShellProperty<ulong?>(() => File.Exists(path) ? (ulong)new FileInfo(path).Length : null);

        public ShellProperty<string> Title { get; } = ShellPropertyStore.Create(path, "System.Title", string.Empty);
        public ShellProperty<string> Subject { get; } = ShellPropertyStore.Create(path, "System.Subject", string.Empty);
        public ShellProperty<string> Comment { get; } = ShellPropertyStore.Create(path, "System.Comment", string.Empty);
        public ShellProperty<string[]> Keywords { get; } = ShellPropertyStore.Create(path, "System.Keywords", Array.Empty<string>());

        public PhotoProperties Photo { get; } = new PhotoProperties(path);
        public MediaProperties Media { get; } = new MediaProperties(path);
        public GpsProperties GPS { get; } = new GpsProperties(path);
    }

    public sealed class PhotoProperties(string path)
    {
        public ShellProperty<string> CameraManufacturer { get; } = ShellPropertyStore.Create(path, "System.Photo.CameraManufacturer", string.Empty);
        public ShellProperty<string> CameraModel { get; } = ShellPropertyStore.Create(path, "System.Photo.CameraModel", string.Empty);
        public ShellProperty<DateTime?> DateTaken { get; } = ShellPropertyStore.Create<DateTime?>(path, "System.Photo.DateTaken", null);
        public ShellProperty<ushort?> Orientation { get; } = ShellPropertyStore.Create<ushort?>(path, "System.Photo.Orientation", null);
    }

    public sealed class MediaProperties(string path)
    {
        public ShellProperty<DateTime?> DateEncoded { get; } = ShellPropertyStore.Create<DateTime?>(path, "System.Media.DateEncoded", null);
    }

    public sealed class GpsProperties(string path)
    {
        public ShellProperty<string> AreaInformation { get; } = ShellPropertyStore.Create(path, "System.GPS.AreaInformation", string.Empty);
        public ShellProperty<double[]> Latitude { get; } = ShellPropertyStore.Create(path, "System.GPS.Latitude", Array.Empty<double>());
        public ShellProperty<double[]> Longitude { get; } = ShellPropertyStore.Create(path, "System.GPS.Longitude", Array.Empty<double>());
        public ShellProperty<string> LatitudeRef { get; } = ShellPropertyStore.Create(path, "System.GPS.LatitudeRef", string.Empty);
        public ShellProperty<string> LongitudeRef { get; } = ShellPropertyStore.Create(path, "System.GPS.LongitudeRef", string.Empty);
        public ShellProperty<uint[]> LatitudeNumerator { get; } = ShellPropertyStore.Create(path, "System.GPS.LatitudeNumerator", Array.Empty<uint>());
        public ShellProperty<uint[]> LongitudeNumerator { get; } = ShellPropertyStore.Create(path, "System.GPS.LongitudeNumerator", Array.Empty<uint>());
        public ShellProperty<uint[]> LatitudeDenominator { get; } = ShellPropertyStore.Create(path, "System.GPS.LatitudeDenominator", Array.Empty<uint>());
        public ShellProperty<uint[]> LongitudeDenominator { get; } = ShellPropertyStore.Create(path, "System.GPS.LongitudeDenominator", Array.Empty<uint>());
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
        public static ShellProperty<T> Create<T>(string path, string canonicalName, T defaultValue)
        {
            return new ShellProperty<T>(
                () => GetValue(path, canonicalName, defaultValue),
                value => SetValue(path, canonicalName, value));
        }

        private static T GetValue<T>(string path, string canonicalName, T defaultValue)
        {
            if (!TryGetPropertyKey(canonicalName, out PropertyKey propertyKey))
            {
                return defaultValue;
            }

            IPropertyStore? propertyStore = null;
            PropVariant propVariant = default;
            try
            {
                propertyStore = GetPropertyStore(path, GetPropertyStoreFlags.Default);
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
                PropVariantClear(ref propVariant);
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
                    propertyStore.Commit();
                }
            }
            catch
            {
            }
            finally
            {
                PropVariantClear(ref propVariant);
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
        private const ushort VtFileTime = 64;
        private const ushort VtLpWStr = 31;
        private const ushort VtVector = 0x1000;
        private const ushort VtR8 = 5;

        [FieldOffset(0)]
        private ushort _valueType;

        [FieldOffset(8)]
        private int _intValue;

        [FieldOffset(8)]
        private ushort _ushortValue;

        [FieldOffset(8)]
        private uint _uintValue;

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

            return new PropVariant { _valueType = VtEmpty };
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
