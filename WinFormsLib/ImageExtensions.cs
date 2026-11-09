using System.Drawing.Imaging;

namespace WinFormsLib
{
    /// <summary>Provides helpers for encoding images as bytes or Base64 text.</summary>
    public static class ImageExtensions
    {
        /// <summary>Encodes the image in the requested file format.</summary>
        /// <param name="super">The image to encode.</param>
        /// <param name="format">The output format, or null to use BMP.</param>
        /// <returns>The encoded image bytes.</returns>
        public static byte[] ToByteArray(this Image super, ImageFormat? format = null)
        {
            format ??= ImageFormat.Bmp;
            using MemoryStream ms = new();
            super.Save(ms, format);
            return ms.ToArray();
        }

        /// <summary>Encodes the image as BMP and returns its Base64 representation as a JSON string.</summary>
        /// <param name="super">The image to encode.</param>
        public static string ToJson(this Image super) => System.Text.Json.JsonSerializer.Serialize(super.ToByteArray());
    }
}
