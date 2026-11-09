using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    /// <summary>Provides path, text I/O, and execution helpers for files.</summary>
    public static class FileInfoExtensions
    {
        private class FileInfoJsonConverter : JsonConverter<FileInfo>
        {
            public override FileInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s)
                    ? new(s)
                    : throw new ArgumentException($"'{nameof(reader)}' cannot be null or empty.", nameof(reader));
            }

            public override void Write(Utf8JsonWriter writer, FileInfo fileInfo, JsonSerializerOptions options) => writer.WriteStringValue(fileInfo.FullName);
        }
        /// <summary>Gets a converter that represents a file as its full path in a JSON string.</summary>
        public static JsonConverter JsonConverter => new FileInfoJsonConverter();

        /// <summary>Gets the file name without its extension.</summary>
        /// <param name="super">The file to inspect.</param>
        public static string GetNameWithoutExtension(this FileInfo super) => Utils.GetFileNameWithoutExtension(super.FullName);

        /// <summary>Gets the directory path for the file.</summary>
        /// <param name="super">The file to inspect.</param>
        public static string GetDirectoryPath(this FileInfo super) => Utils.GetDirectoryPath(super.FullName);

        /// <summary>Gets the path of the file's parent directory.</summary>
        /// <param name="super">The file to inspect.</param>
        public static string GetParentPath(this FileInfo super) => Utils.GetParentPath(super.FullName);

        /// <summary>Reads the file as text and encodes the result as UTF-8 bytes.</summary>
        /// <param name="super">The file to read.</param>
        /// <returns>The encoded text, or an empty array if reading returns no text.</returns>
        public static byte[] Read(this FileInfo super) => ReadText(super).ToBytes();

        /// <summary>Decodes the bytes as text using the default encoding, then writes UTF-8 text.</summary>
        /// <param name="super">The destination file.</param>
        /// <param name="bytes">The encoded text to write.</param>
        /// <param name="append">Whether to append the text followed by a newline.</param>
        /// <returns><see langword="true"/> if the write succeeds, or <see langword="false"/> if the text writer reports an error.</returns>
        public static bool Write(this FileInfo super, byte[] bytes, bool append = false) => super.WriteText(Encoding.Default.GetString(bytes), append);

        /// <summary>Reads text from the file.</summary>
        /// <param name="super">The file to read.</param>
        /// <param name="lastLineOnly">Whether to read only the final UTF-8 line, without its line terminator.</param>
        /// <returns>The text read, or an empty string if no text was read or an I/O error occurred.</returns>
        public static string ReadText(this FileInfo super, bool lastLineOnly = false)
        {
            try
            {
                if (!lastLineOnly)
                {
                    using StreamReader reader = new(super.FullName, Encoding.UTF8);
                    return reader.ReadToEnd();
                }

                using FileStream stream = File.OpenRead(super.FullName);
                return ReadLastUtf8Line(stream);
            }
            catch (IOException e)
            {
                Debug.WriteLine($"Failed to read text: {e.Message}");
                return string.Empty;
            }
        }

        private static string ReadLastUtf8Line(FileStream stream)
        {
            long end = stream.Length;
            if (end == 0)
            {
                return string.Empty;
            }

            // Skip one trailing line terminator before looking for the start of the final line.
            stream.Position = end - 1;
            int lastByte = stream.ReadByte();
            if (lastByte is '\r' or '\n')
            {
                end--;
                if (lastByte == '\n' && end > 0)
                {
                    stream.Position = end - 1;
                    if (stream.ReadByte() == '\r')
                    {
                        end--;
                    }
                }
            }

            Span<byte> buffer = stackalloc byte[4096];
            long start = 0;
            while (end > 0)
            {
                int count = (int)Math.Min(end, buffer.Length);
                long blockStart = end - count;
                stream.Position = blockStart;
                Span<byte> block = buffer[..count];
                stream.ReadExactly(block);
                int separator = block.LastIndexOfAny((byte)'\r', (byte)'\n');
                if (separator >= 0)
                {
                    start = blockStart + separator + 1;
                    break;
                }

                end = blockStart;
            }

            stream.Position = start;
            Encoding encoding = start == 0 ? Encoding.UTF8 : new UTF8Encoding(false);
            using StreamReader reader = new(stream, encoding, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            return reader.ReadLine() ?? string.Empty;
        }

        /// <summary>Writes UTF-8 text, creating the destination directory if needed.</summary>
        /// <param name="super">The destination file.</param>
        /// <param name="s">The text to write.</param>
        /// <param name="appendLine">Whether to append the text followed by a newline. Otherwise, the existing contents are replaced.</param>
        /// <returns><see langword="true"/> if the write succeeds, or <see langword="false"/> if opening or writing the file fails.</returns>
        /// <remarks>Errors while creating the destination directory propagate to the caller.</remarks>
        public static bool WriteText(this FileInfo super, string s, bool appendLine = false)
        {
            if (super.DirectoryName is string dp && new DirectoryInfo(dp) is DirectoryInfo di)
            {
                di.Create();
            }
            try
            {
                using StreamWriter sw = new(super.FullName, appendLine, Encoding.UTF8);
                sw.AutoFlush = true;
                if (appendLine)
                {
                    sw.WriteLine(s);
                }
                else
                {
                    sw.Write(s);
                }
                sw.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>Attempts to rename the file in its current directory while retaining its extension.</summary>
        /// <param name="super">The file to rename.</param>
        /// <param name="name">The new name without an extension.</param>
        /// <returns>The destination path after a successful move, or the original path if no move occurs.</returns>
        public static string Rename(this FileInfo super, string name) => Utils.RenameFile(super.FullName, name);

        /// <summary>Attempts to launch an existing executable file.</summary>
        /// <param name="super">The executable to launch.</param>
        /// <exception cref="IOException">The file does not exist.</exception>
        /// <exception cref="ArgumentException">The file does not have the expected executable extension.</exception>
        public static void Execute(this FileInfo super) => Utils.Execute(super.FullName);
    }
}
