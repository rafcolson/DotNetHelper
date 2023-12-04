using System.Text.Json;
using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Collections.Specialized;

namespace WinFormsLib
{
    public class Map<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
    {
        private class ReverseComparer : IComparer
        {
            public int Compare(object? x, object? y) => new CaseInsensitiveComparer().Compare(y, x);
        }

        private class MapJsonConverter : JsonConverter<Map<TKey, TValue>>
        {
            public override Map<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string json ? new(json) : new();
            }
            public override void Write(Utf8JsonWriter writer, Map<TKey, TValue> map, JsonSerializerOptions options)
            {
                writer.WriteStringValue(map.ToString());
            }
        }

        public static JsonConverter JsonConverter => new MapJsonConverter();

        public JsonSerializerOptions JsonSerializerOptions => new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public Map() { }

        public Map(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            if (!json.IsDictionary())
            {
                json = $"{Chars.LEFT_CURLY_BRACE}{json}{Chars.RIGHT_CURLY_BRACE}";
            }
            if (JsonSerializer.Deserialize<IDictionary<TKey, TValue>>(json, JsonSerializerOptions) is IDictionary<TKey, TValue> id)
            {
                Union(id);
            }
        }

        public Map(IDictionary<TKey, TValue> id) => Union(id);

        public void Union(IDictionary<TKey, TValue> id)
        {
            if (id != null)
            {
                foreach (KeyValuePair<TKey, TValue> kvp in id)
                {
                    this[kvp.Key] = kvp.Value;
                }
                return;
            }
            throw new ArgumentNullException(nameof(id));
        }

        public void Replace(IDictionary<TKey, TValue> id)
        {
            Clear();
            Union(id);
        }

        public Map<TKey, TValue> Intersect(IEnumerable<TKey> keys)
        {
            Map<TKey, TValue> m = new(this);
            m.IntersectWith(keys);
            return m;
        }

        public Map<TKey, TValue> Except(IEnumerable<TKey> keys)
        {
            Map<TKey, TValue> m = new(this);
            m.ExceptWith(keys);
            return m;
        }

        public bool ExceptWith(IEnumerable<TKey> keys)
        {
            HashSet<TKey> hs = Keys.ToHashSet();
            hs.IntersectWith(keys);
            if (hs.Any())
            {
                foreach (TKey k in hs)
                {
                    Remove(k);
                }
                return true;
            }
            return false;
        }

        public bool IntersectWith(IEnumerable<TKey> keys)
        {
            HashSet<TKey> hs = Keys.ToHashSet();
            hs.ExceptWith(keys);
            if (hs.Any())
            {
                foreach (TKey k in hs)
                {
                    Remove(k);
                }
                return true;
            }
            return false;
        }

        public bool IsSubsetOf(IDictionary<TKey, TValue> other) => Keys.ToHashSet().IsSubsetOf(other.Keys);

        public void Sort(bool ascending = true)
        {
            TKey[] keys = Keys.ToArray();
            if (ascending)
            {
                Array.Sort(keys);
            }
            else
            {
                Array.Sort(keys, new ReverseComparer());
            }
            Map<TKey, TValue> m = new(this);
            Clear();
            foreach (TKey k in keys)
            {
                Add(k, m[k]);
            }
        }

        public bool Put(TKey k, TValue v)
        {
            bool result = !ContainsKey(k);
            this[k] = v;
            return result;
        }

        public TKey[] GetKeys(TValue v)
        {
            List<TKey> l = new();
            List<KeyValuePair<TKey, TValue>> ie = new(this);
            l.AddRange(ie.Where(kvp => v != null && v.Equals(kvp.Value)).Select(x => x.Key));
            return l.ToArray();
        }

        public TKey[] GetKeys() => Keys.ToArray();

        public TValue GetValue(TKey k) => this[k];

        public TValue Pop(TKey k)
        {
            TValue v = this[k];
            Remove(k);
            return v;
        }

        public TValue[] GetValues() => Values.ToArray();

        public OrderedDictionary ToOrderedDictionary()
        {
            OrderedDictionary od = new();
            foreach (KeyValuePair<TKey, TValue> kvp in this)
            {
                od.Add(kvp.Key, kvp.Value);
            }
            return od;
        }

        public virtual Map<TKey, TValue> Clone() => new(ToString());

        public override string ToString() => JsonSerializer.Serialize(this, JsonSerializerOptions);
    }
}
