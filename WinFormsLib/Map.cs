using System.Collections;
using System.Collections.Specialized;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    /// <summary>Extends a generic dictionary with key-set operations and JSON conversion.</summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
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

        /// <summary>Gets a converter that represents the map as a JSON string containing a serialized object.</summary>
        public static JsonConverter JsonConverter => new MapJsonConverter();

        /// <summary>Gets a new set of serializer options with indentation and relaxed escaping.</summary>
        public JsonSerializerOptions JsonSerializerOptions => new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        /// <summary>Initializes an empty map.</summary>
        public Map() { }

        /// <summary>Initializes a map from JSON. An empty input creates an empty map.</summary>
        /// <param name="json">A JSON object or its contents without the surrounding braces.</param>
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

        /// <summary>Initializes a map from the supplied entries.</summary>
        /// <param name="id">The source dictionary.</param>
        public Map(IDictionary<TKey, TValue> id) => Union(id);

        /// <summary>Returns the zero-based position of a key in the current key enumeration, or -1 if absent.</summary>
        /// <param name="k">The key to look up or change.</param>
        public int GetIndexOf(TKey k) => Array.IndexOf([.. Keys], k);

        /// <summary>Copies entries from the source, overwriting values for keys already present.</summary>
        /// <param name="id">The source dictionary.</param>
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

        /// <summary>Clears the map and copies entries from the source.</summary>
        /// <param name="id">The source dictionary.</param>
        public void Replace(IDictionary<TKey, TValue> id)
        {
            Clear();
            Union(id);
        }

        /// <summary>Returns a new map containing only entries whose keys are in the supplied sequence.</summary>
        /// <param name="keys">The keys to compare against.</param>
        public Map<TKey, TValue> Intersect(IEnumerable<TKey> keys)
        {
            Map<TKey, TValue> m = new(this);
            _ = m.IntersectWith(keys);
            return m;
        }

        /// <summary>Returns a new map omitting entries whose keys are in the supplied sequence.</summary>
        /// <param name="keys">The keys to compare against.</param>
        public Map<TKey, TValue> Except(IEnumerable<TKey> keys)
        {
            Map<TKey, TValue> m = new(this);
            _ = m.ExceptWith(keys);
            return m;
        }

        /// <summary>Removes entries whose keys are in the supplied sequence.</summary>
        /// <param name="keys">The keys to compare against.</param>
        /// <returns><see langword="true"/> if at least one entry was removed.</returns>
        public bool ExceptWith(IEnumerable<TKey> keys)
        {
            HashSet<TKey> hs = [.. Keys];
            hs.IntersectWith(keys);
            if (hs.Count != 0)
            {
                foreach (TKey k in hs)
                {
                    _ = Remove(k);
                }
                return true;
            }
            return false;
        }

        /// <summary>Removes entries whose keys are absent from the supplied sequence.</summary>
        /// <param name="keys">The keys to compare against.</param>
        /// <returns><see langword="true"/> if at least one entry was removed.</returns>
        public bool IntersectWith(IEnumerable<TKey> keys)
        {
            HashSet<TKey> hs = [.. Keys];
            hs.ExceptWith(keys);
            if (hs.Count != 0)
            {
                foreach (TKey k in hs)
                {
                    _ = Remove(k);
                }
                return true;
            }
            return false;
        }

        /// <summary>Determines whether all keys are present in the other dictionary.</summary>
        /// <param name="other">The dictionary whose keys are checked.</param>
        /// <returns><see langword="true"/> if every key is present in the other dictionary. Values are ignored.</returns>
        public bool IsSubsetOf(IDictionary<TKey, TValue> other) => Keys.ToHashSet().IsSubsetOf(other.Keys);

        /// <summary>Rebuilds the map by inserting its entries in sorted key order.</summary>
        /// <param name="ascending">Whether to sort in ascending order.</param>
        /// <remarks>The underlying dictionary does not contractually guarantee enumeration order.</remarks>
        public void Sort(bool ascending = true)
        {
            TKey[] keys = [.. Keys];
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

        /// <summary>Adds a key and value or replaces the value of an existing key.</summary>
        /// <param name="k">The key to look up or change.</param>
        /// <param name="v">The value to store or look up.</param>
        /// <returns><see langword="true"/> if the key was added, or <see langword="false"/> if an existing value was replaced.</returns>
        public bool Put(TKey k, TValue v)
        {
            bool result = !ContainsKey(k);
            this[k] = v;
            return result;
        }

        /// <summary>Returns keys associated with the supplied non-null value.</summary>
        /// <param name="v">The value to store or look up.</param>
        public TKey[] GetKeys(TValue v)
        {
            List<TKey> l = [];
            List<KeyValuePair<TKey, TValue>> ie = [.. this];
            l.AddRange(ie.Where(kvp => v != null && v.Equals(kvp.Value)).Select(x => x.Key));
            return [.. l];
        }

        /// <summary>Returns a snapshot of the keys in enumeration order.</summary>
        public TKey[] GetKeys() => [.. Keys];

        /// <summary>Gets the value associated with a key.</summary>
        /// <param name="k">The key to look up or change.</param>
        /// <exception cref="KeyNotFoundException">The key is absent.</exception>
        public TValue GetValue(TKey k) => this[k];

        /// <summary>Removes a key and returns its associated value.</summary>
        /// <param name="k">The key to look up or change.</param>
        /// <exception cref="KeyNotFoundException">The key is absent.</exception>
        public TValue Pop(TKey k)
        {
            TValue v = this[k];
            _ = Remove(k);
            return v;
        }

        /// <summary>Returns a snapshot of the values in enumeration order.</summary>
        public TValue[] GetValues() => [.. Values];

        /// <summary>Copies the entries into an ordered dictionary in their current enumeration order.</summary>
        public OrderedDictionary ToOrderedDictionary()
        {
            OrderedDictionary od = [];
            foreach (KeyValuePair<TKey, TValue> kvp in this)
            {
                od.Add(kvp.Key, kvp.Value);
            }
            return od;
        }

        /// <summary>Copies the map by serializing and deserializing its entries.</summary>
        public virtual Map<TKey, TValue> Clone() => new(ToString());

        /// <summary>Returns the indented JSON representation of the map.</summary>
        public override string ToString() => JsonSerializer.Serialize(this, JsonSerializerOptions);
    }
}
