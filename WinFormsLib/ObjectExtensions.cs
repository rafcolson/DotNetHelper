using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    public static class ObjectExtensions
    {
        public static T? ToEnum<T>(this object super) where T : Enum
        {
            foreach (T t in Enum.GetValues(typeof(T)))
            {
                if (super == t.GetValue<object>())
                {
                    return t;
                }
            }
            return default;
        }

        public static string ToJson(this object super, JsonSerializerOptions? options = null)
        {
            options ??= new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            return super is Color c ? c.ToHtml() : super is DateTime dt ? dt.ToDateTimeString() : JsonSerializer.Serialize(super, options);
        }

        public static string ToJson(this object super, JsonConverter[] converters, JavaScriptEncoder? encoder = null)
        {
            encoder ??= JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializerOptions? options = new() { Encoder = encoder };
            foreach (JsonConverter converter in converters) { options.Converters.Add(converter); }
            return super.ToJson(options);
        }

        public static string ToJson(this object super, JsonConverter converter, JavaScriptEncoder? encoder = null)
        {
            return super.ToJson(new JsonConverter[] { converter }, encoder);
        }
    }
}
