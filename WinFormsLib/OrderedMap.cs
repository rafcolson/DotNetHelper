using System.Text.Json;
using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Collections.Specialized;

namespace WinFormsLib
{
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

        public static JsonConverter JsonConverter => new OrderedMapJsonConverter();

        public JsonSerializerOptions JsonSerializerOptions => new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public OrderedMap() { }

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

        public OrderedMap(IDictionary<TKey, TValue> id) => Union(id);

        public OrderedMap(OrderedMap<TKey, TValue> om) => Union(om);

        public void Union(OrderedMap<TKey, TValue> om)
        {
            if (om != null)
            {
                foreach (KeyValuePair<TKey, TValue> kvp in om)
                {
                    this[kvp.Key] = kvp.Value;
                }
                return;
            }
            throw new ArgumentNullException(nameof(om));
        }

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

        public void Replace(OrderedMap<TKey, TValue> om)
        {
            Clear();
            Union(om);
        }

        public void Replace(IDictionary<TKey, TValue> id)
        {
            Clear();
            Union(id);
        }

        public OrderedMap<TKey, TValue> Intersect(IEnumerable<TKey> keys)
        {
            OrderedMap<TKey, TValue> m = new(this);
            m.IntersectWith(keys);
            return m;
        }

        public OrderedMap<TKey, TValue> Except(IEnumerable<TKey> keys)
        {
            OrderedMap<TKey, TValue> om = new(this);
            om.ExceptWith(keys);
            return om;
        }

        public bool ExceptWith(IEnumerable<TKey> keys)
        {
            HashSet<TKey> hs = GetKeys().ToHashSet();
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
            HashSet<TKey> hs = GetKeys().ToHashSet();
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

        public bool IsSubsetOf(IDictionary<TKey, TValue> other) => GetKeys().ToHashSet().IsSubsetOf(other.Keys);

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

        public bool Put(TKey k, TValue v)
        {
            bool result = !Contains(k);
            this[k] = v;
            return result;
        }

        public TKey? GetFirstKey() => GetKeys().ElementAtOrDefault(0);

        public TKey? GetLastKey() => GetKeys().ElementAtOrDefault(Count - 1);

        public TKey[] GetKeys(TValue v)
        {
            List<TKey> l = new();
            List<KeyValuePair<TKey, TValue>> ie = new((IDictionary<TKey, TValue>)this);
            l.AddRange(ie.Where(kvp => v != null && v.Equals(kvp.Value)).Select(x => x.Key));
            return l.ToArray();
        }

        public TKey[] GetKeys() => Keys.Cast<TKey>().ToArray();

        public TValue? GetValue(TKey k) => (TValue?)this[k];

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

        public TValue? GetFirstValue() => GetValues().ElementAtOrDefault(0);

        public TValue? GetLastValue() => GetValues().ElementAtOrDefault(Count - 1);

        public TValue[] GetValues() => Values.Cast<TValue>().ToArray();

        public int GetIndexOf(TKey k) => Array.IndexOf(GetKeys(), k);

        public int GetFirstIndexOf(TValue v) => Array.IndexOf(GetValues(), v);

        public int GetLastIndexOf(TValue v) => Array.LastIndexOf(GetValues(), v);

        public virtual OrderedMap<TKey, TValue> Clone() => new(ToString());

        public Dictionary<TKey, TValue> ToDictionary() => new((IDictionary<TKey, TValue>)Clone());

        public override string ToString() => JsonSerializer.Serialize(this, JsonSerializerOptions);
    }
}
