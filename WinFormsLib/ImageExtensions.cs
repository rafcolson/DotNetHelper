using System.Drawing.Imaging;

namespace WinFormsLib
{
    public static class ImageExtensions
    {
        public static byte[] ToByteArray(this Image super, ImageFormat? format = null)
        {
            format ??= ImageFormat.Bmp;
            using MemoryStream ms = new();
            super.Save(ms, format);
            return ms.ToArray();
        }

        public static string ToJson(this Image super) => Convert.ToBase64String(super.ToByteArray());
    }
}
