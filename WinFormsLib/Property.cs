using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    public class Property
    {
        private class PropertyJsonConverter : JsonConverter<Property>
        {
            public override Property Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s)
                    ? (Property)new(s)
                    : throw new ArgumentException($"'{nameof(reader)}' cannot be null or empty.", nameof(reader));
            }
            public override void Write(Utf8JsonWriter writer, Property property, JsonSerializerOptions options) => writer.WriteStringValue(property.ToString());
        }

        private static JsonSerializerOptions JsonSerializerOptions => new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public static JsonConverter JsonConverter => new PropertyJsonConverter();

        public string Key { get; set; }
        public object? Value { get; set; }

        public Property(string key, object? value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
            }
            Key = key;
            Value = value;
        }

        public Property(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException($"'{nameof(json)}' cannot be null or empty.", nameof(json));
            }
            else if (JsonSerializer.Deserialize<Property>(json, JsonSerializerOptions) is Property prop)
            {
                Key = prop.Key;
                Value = prop.Value;
            }
            else
            {
                throw new ArgumentException($"'{nameof(json)}' invalid.", nameof(json));
            }
        }

        public Property Clone() => new(ToString());

        public override string ToString() => JsonSerializer.Serialize(this, JsonSerializerOptions);

        public override bool Equals(object? obj) => obj is Property prop && Key == prop.Key && Value == prop.Value;

        public override int GetHashCode() => HashCode.Combine(Key, Value);
    }
}
