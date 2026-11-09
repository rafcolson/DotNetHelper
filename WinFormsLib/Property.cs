using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    /// <summary>Represents a mutable property with a string key and an object value.</summary>
    public class Property
    {
        private class PropertyJsonConverter : JsonConverter<Property>
        {
            public override Property Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string s && !string.IsNullOrEmpty(s)
                    ? new(s)
                    : throw new ArgumentException($"'{nameof(reader)}' cannot be null or empty.", nameof(reader));
            }
            public override void Write(Utf8JsonWriter writer, Property property, JsonSerializerOptions options) => writer.WriteStringValue(property.ToString());
        }

        private static JsonSerializerOptions JsonSerializerOptions => new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        /// <summary>Gets a converter that stores the serialized property object inside a JSON string.</summary>
        public static JsonConverter JsonConverter => new PropertyJsonConverter();

        /// <summary>Gets or sets the property key.</summary>
        public string Key { get; set; }
        /// <summary>Gets or sets the property value.</summary>
        public object? Value { get; set; }

        /// <summary>Initializes a property with the specified key and value.</summary>
        /// <param name="key">The nonempty property key.</param>
        /// <param name="value">The property value, which may be null.</param>
        /// <exception cref="ArgumentException"><paramref name="key"/> is null or empty.</exception>
        [JsonConstructor]
        public Property(string key, object? value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
            }
            Key = key;
            Value = value;
        }

        /// <summary>Attempts to initialize a property from a JSON object.</summary>
        /// <param name="json">The serialized property object.</param>
        /// <exception cref="ArgumentException">The input is null or empty, or deserializes to null.</exception>
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

        /// <summary>Copies the property by serializing and deserializing it.</summary>
        /// <remarks>A non-null value is deserialized as a <see cref="JsonElement"/>.</remarks>
        public Property Clone() => new(ToString());

        /// <summary>Returns a JSON representation of the property.</summary>
        public override string ToString() => JsonSerializer.Serialize(this, JsonSerializerOptions);

        /// <summary>Compares two properties by key and value reference.</summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> if the keys are equal and both values are the same reference or both are null.</returns>
        public override bool Equals(object? obj) => obj is Property prop && Key == prop.Key && Value == prop.Value;

        /// <summary>Combines the hash codes of the key and value.</summary>
        public override int GetHashCode() => HashCode.Combine(Key, Value);
    }
}
