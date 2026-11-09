using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    /// <summary>Provides helpers for enum attribute lookup and object serialization.</summary>
    public static class ObjectExtensions
    {
        /// <summary>Finds the first enum member whose attribute value equals the supplied value.</summary>
        /// <typeparam name="T">The enum type to search.</typeparam>
        /// <param name="super">The object to compare against the attribute values.</param>
        /// <returns>The matching member, or null if there is no match.</returns>
        public static T? ToEnum<T>(this object super) where T : struct, Enum
        {
            foreach (T t in Enum.GetValues<T>())
            {
                if (object.Equals(super, t.GetValue<object>()))
                {
                    return t;
                }
            }
            return default;
        }

        /// <summary>Serializes the object to JSON.</summary>
        /// <param name="super">The object to serialize.</param>
        /// <param name="options">The serializer options, or null to use relaxed JSON escaping.</param>
        /// <remarks>Colors and dates use their HTML color or date-time format as a JSON string unless a converter is registered for their type.</remarks>
        public static string ToJson(this object super, JsonSerializerOptions? options = null)
        {
            options ??= new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            if (super is Color or DateTime)
            {
                Type type = super.GetType();
                if (options.Converters.Any(converter => converter.CanConvert(type)))
                {
                    return JsonSerializer.Serialize(super, type, options);
                }

                string formatted = super is Color color ? color.ToHtml() : ((DateTime)super).ToDateTimeString();
                return JsonSerializer.Serialize(formatted, options);
            }

            return JsonSerializer.Serialize(super, options);
        }

        /// <summary>Serializes the object to JSON with the supplied converters.</summary>
        /// <param name="super">The object to serialize.</param>
        /// <param name="converters">The converters to register in the serializer options.</param>
        /// <param name="encoder">The JSON encoder, or null to use relaxed escaping.</param>
        public static string ToJson(this object super, JsonConverter[] converters, JavaScriptEncoder? encoder = null)
        {
            encoder ??= JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializerOptions? options = new() { Encoder = encoder };
            foreach (JsonConverter converter in converters) { options.Converters.Add(converter); }
            return super.ToJson(options);
        }

        /// <summary>Serializes the object to JSON with the supplied converter.</summary>
        /// <param name="super">The object to serialize.</param>
        /// <param name="converter">The converter to register in the serializer options.</param>
        /// <param name="encoder">The JSON encoder, or null to use relaxed escaping.</param>
        public static string ToJson(this object super, JsonConverter converter, JavaScriptEncoder? encoder = null)
        {
            return super.ToJson([converter], encoder);
        }
    }
}
