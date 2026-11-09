using System.Collections;
using System.Collections.Specialized;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsLib
{
    /// <summary>Stores key-value pairs in an explicitly ordered collection.</summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public class OrderedMap<TKey, TValue> : OrderedDictionary where TKey : notnull
    {
        private class ReverseComparer : IComparer
        {
            public int Compare(object? x, object? y) => new CaseInsensitiveComparer().Compare(y, x);
        }

        private class OrderedMapJsonConverter : JsonConverter<OrderedMap<TKey, TValue>>
        {
            public override OrderedMap<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() is string json ? new(json) : new();
            }
            public override void Write(Utf8JsonWriter writer, OrderedMap<TKey, TValue> orderedMap, JsonSerializerOptions options)
            {
                writer.WriteStringValue(orderedMap.ToString());
            }
        }

        /// <summary>Gets a converter that represents the map as a JSON string containing a serialized object.</summary>
        public static JsonConverter JsonConverter => new OrderedMapJsonConverter();

        /// <summary>Gets a new set of serializer options with indentation and relaxed escaping.</summary>
        public JsonSerializerOptions JsonSerializerOptions => new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        /// <summary>Initializes an empty map.</summary>
        public OrderedMap() { }

        /// <summary>Initializes a map from JSON. An empty input creates an empty map.</summary>
        /// <param name="json">A JSON object or its contents without the surrounding braces.</param>
        public OrderedMap(string json)
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
        public OrderedMap(IDictionary<TKey, TValue> id) => Union(id);

        /// <summary>Initializes a map from the supplied entries.</summary>
        /// <param name="om">The source ordered map.</param>
        public OrderedMap(OrderedMap<TKey, TValue> om) => Union(om);

        /// <summary>Copies entries from the source, overwriting values for keys already present.</summary>
        /// <param name="om">The source ordered map.</param>
        /// <remarks>New keys are appended in source order. Existing keys retain their positions.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="om"/> is null.</exception>
        public void Union(OrderedMap<TKey, TValue> om)
        {
            ArgumentNullException.ThrowIfNull(om);
            if (ReferenceEquals(this, om))
            {
                return;
            }

            foreach (DictionaryEntry entry in om)
            {
                this[entry.Key] = entry.Value;
            }
        }

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
        /// <param name="om">The source ordered map.</param>
        public void Replace(OrderedMap<TKey, TValue> om)
        {
            Clear();
            Union(om);
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
        public OrderedMap<TKey, TValue> Intersect(IEnumerable<TKey> keys)
        {
            OrderedMap<TKey, TValue> m = new(this);
            _ = m.IntersectWith(keys);
            return m;
        }

        /// <summary>Returns a new map omitting entries whose keys are in the supplied sequence.</summary>
        /// <param name="keys">The keys to compare against.</param>
        public OrderedMap<TKey, TValue> Except(IEnumerable<TKey> keys)
        {
            OrderedMap<TKey, TValue> om = new(this);
            _ = om.ExceptWith(keys);
            return om;
        }

        /// <summary>Removes entries whose keys are in the supplied sequence.</summary>
        /// <param name="keys">The keys to compare against.</param>
        /// <returns><see langword="true"/> if at least one entry was removed.</returns>
        public bool ExceptWith(IEnumerable<TKey> keys)
        {
            HashSet<TKey> hs = [.. GetKeys()];
            hs.IntersectWith(keys);
            if (hs.Count != 0)
            {
                foreach (TKey k in hs)
                {
                    Remove(k);
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
            HashSet<TKey> hs = [.. GetKeys()];
            hs.ExceptWith(keys);
            if (hs.Count != 0)
            {
                foreach (TKey k in hs)
                {
                    Remove(k);
                }
                return true;
            }
            return false;
        }

        /// <summary>Determines whether all keys are present in the other dictionary.</summary>
        /// <param name="other">The dictionary whose keys are checked.</param>
        /// <returns><see langword="true"/> if every key is present in the other dictionary. Values are ignored.</returns>
        public bool IsSubsetOf(IDictionary<TKey, TValue> other) => GetKeys().ToHashSet().IsSubsetOf(other.Keys);

        /// <summary>Rebuilds the map by inserting its entries in sorted key order.</summary>
        /// <param name="ascending">Whether to sort in ascending order.</param>
        public void Sort(bool ascending = true)
        {
            TKey[] keys = GetKeys();
            if (ascending)
            {
                Array.Sort(keys);
            }
            else
            {
                Array.Sort(keys, new ReverseComparer());
            }
            OrderedMap<TKey, TValue> om = new(this);
            Clear();
            foreach (TKey k in keys)
            {
                Add(k, om[k]);
            }
        }

        /// <summary>Adds a key and value or replaces the value of an existing key.</summary>
        /// <param name="k">The key to look up or change.</param>
        /// <param name="v">The value to store or look up.</param>
        /// <returns><see langword="true"/> if the key was added, or <see langword="false"/> if an existing value was replaced.</returns>
        public bool Put(TKey k, TValue v)
        {
            bool result = !Contains(k);
            this[k] = v;
            return result;
        }

        /// <summary>Returns the first key or the default key value if the map is empty.</summary>
        public TKey? GetFirstKey() => GetKeys().ElementAtOrDefault(0);

        /// <summary>Returns the last key or the default key value if the map is empty.</summary>
        public TKey? GetLastKey() => GetKeys().ElementAtOrDefault(Count - 1);

        /// <summary>Returns keys associated with the supplied value in map order.</summary>
        /// <param name="v">The value to look up.</param>
        /// <returns>The matching keys, or an empty array if the value is null or has no matches.</returns>
        public TKey[] GetKeys(TValue v)
        {
            if (v is null)
            {
                return [];
            }

            List<TKey> matchingKeys = [];
            foreach (DictionaryEntry entry in this)
            {
                if (v.Equals(entry.Value))
                {
                    matchingKeys.Add((TKey)entry.Key);
                }
            }
            return [.. matchingKeys];
        }

        /// <summary>Returns a snapshot of the keys in enumeration order.</summary>
        public TKey[] GetKeys() => [.. Keys.Cast<TKey>()];

        /// <summary>Gets the value associated with a key.</summary>
        /// <param name="k">The key to look up or change.</param>
        /// <remarks>An absent key yields <see langword="null"/> for nullable value types or reference types. For non-nullable value types, the cast can throw.</remarks>
        public TValue? GetValue(TKey k) => (TValue?)this[k];

        /// <summary>Removes a key and returns its associated value.</summary>
        /// <param name="k">The key to look up or change.</param>
        /// <remarks>An absent key yields <see langword="null"/> for nullable value types or reference types. For non-nullable value types, the cast can throw.</remarks>
        public TValue? Pop(TKey k)
        {
            if (k != null)
            {
                TValue? v = GetValue(k);
                Remove(k);
                return v;
            }
            throw new ArgumentNullException(nameof(k));
        }

        /// <summary>Returns the first value or the default value if the map is empty.</summary>
        public TValue? GetFirstValue() => GetValues().ElementAtOrDefault(0);

        /// <summary>Returns the last value or the default value if the map is empty.</summary>
        public TValue? GetLastValue() => GetValues().ElementAtOrDefault(Count - 1);

        /// <summary>Returns a snapshot of the values in enumeration order.</summary>
        public TValue[] GetValues() => [.. Values.Cast<TValue>()];

        /// <summary>Returns the zero-based position of a key in the current key enumeration, or -1 if absent.</summary>
        /// <param name="k">The key to look up or change.</param>
        public int GetIndexOf(TKey k) => Array.IndexOf(GetKeys(), k);

        /// <summary>Returns the zero-based index of the first matching value, or -1 if absent.</summary>
        /// <param name="v">The value to store or look up.</param>
        public int GetFirstIndexOf(TValue v) => Array.IndexOf(GetValues(), v);

        /// <summary>Returns the zero-based index of the last matching value, or -1 if absent.</summary>
        /// <param name="v">The value to store or look up.</param>
        public int GetLastIndexOf(TValue v) => Array.LastIndexOf(GetValues(), v);

        /// <summary>Copies the map by serializing and deserializing its entries.</summary>
        public virtual OrderedMap<TKey, TValue> Clone() => new(ToString());

        /// <summary>Copies the entries into a new generic dictionary.</summary>
        /// <remarks>Reference-type values are shared with the original map.</remarks>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dictionary = new(Count);
            foreach (DictionaryEntry entry in this)
            {
                dictionary.Add((TKey)entry.Key, (TValue)entry.Value!);
            }
            return dictionary;
        }

        /// <summary>Returns the indented JSON representation of the map.</summary>
        public override string ToString() => JsonSerializer.Serialize(this, JsonSerializerOptions);
    }
}
