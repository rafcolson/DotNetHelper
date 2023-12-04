using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    public static class FileInfoExtensions
    {
        private class FileInfoJsonConverter : JsonConverter<FileInfo>
        {
            public override FileInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s)
                    ? (FileInfo)new(s)
                    : throw new ArgumentException($"'{nameof(reader)}' cannot be null or empty.", nameof(reader));
            }

            public override void Write(Utf8JsonWriter writer, FileInfo fileInfo, JsonSerializerOptions options) => writer.WriteStringValue(fileInfo.FullName);
        }
        public static JsonConverter JsonConverter => new FileInfoJsonConverter();

        public static string GetNameWithoutExtension(this FileInfo super) => Utils.GetFileNameWithoutExtension(super.FullName);

        public static string GetDirectoryPath(this FileInfo super) => Utils.GetDirectoryPath(super.FullName);

        public static string GetParentPath(this FileInfo super) => Utils.GetParentPath(super.FullName);

        public static byte[] Read(this FileInfo super) => ReadText(super).ToBytes();

        public static bool Write(this FileInfo super, byte[] bytes, bool append = false) => super.WriteText(Encoding.Default.GetString(bytes), append);

        public static string ReadText(this FileInfo super, bool lastLineOnly = false)
        {
            string s = Constants.EMPTY_STRING;
            try
            {
                if (lastLineOnly)
                {
                    using Stream fs = File.OpenRead(super.FullName);
                    if (fs.Length != 0)
                    {
                        fs.Position = fs.Length - 1;
                        int byteFromFile = fs.ReadByte();
                        if (byteFromFile == Chars.LINE_FEED)
                        {
                            fs.Position--;
                            while (fs.Position > 0)
                            {
                                fs.Position--;
                                byteFromFile = fs.ReadByte();
                                if (byteFromFile != -1)
                                {
                                    throw new IOException($"Error reading from file {super.FullName}");
                                }
                                else if (byteFromFile == Chars.LINE_FEED)
                                {
                                    break;
                                }
                                fs.Position--;
                            }
                            byte[] bytes = new BinaryReader(fs).ReadBytes((int)(fs.Length - fs.Position));
                            s = Encoding.UTF8.GetString(bytes);
                        }
                    }
                    fs.Close();
                }
                else
                {
                    using StreamReader sr = new(super.FullName, Encoding.UTF8);
                    s = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (IOException e)
            {
                Debug.WriteLine($"Failed to read text: {e.Message}");
            }
            return s;
        }

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

        public static string Rename(this FileInfo super, string name) => Utils.RenameFile(super.FullName, name);

        public static void Execute(this FileInfo super) => Utils.Execute(super.FullName);
    }
}
