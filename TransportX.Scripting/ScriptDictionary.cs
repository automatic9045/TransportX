using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Scripting
{
    internal class ScriptDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyScriptDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> Source = [];
        private ICollection<KeyValuePair<TKey, TValue>> SourceAsCollection => Source;

        private readonly IErrorCollector ErrorCollector;
        private readonly string ItemName;
        private readonly Func<TKey, TValue> DefaultFactory;

        public TValue this[TKey key]
        {
            get
            {
                GetValue(key, out TValue value);
                return value;
            }
            set => Source[key] = value;
        }

        public ICollection<TKey> Keys => Source.Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        public ICollection<TValue> Values => Source.Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
        public int Count => Source.Count;
        public bool IsReadOnly => false;

        public ScriptDictionary(IErrorCollector errorCollector, string itemName, Func<TKey, TValue> defaultFactory)
        {
            ErrorCollector = errorCollector;
            ItemName = itemName;
            DefaultFactory = defaultFactory;
        }

        public bool GetValue(TKey key, out TValue value)
        {
            if (Source.TryGetValue(key, out TValue? sourceValue))
            {
                value = sourceValue;
                return true;
            }

            ScriptError error = new(ErrorLevel.Error, $"{ItemName} '{key}' が見つかりません。");
            ErrorCollector.Report(error);

            value = DefaultFactory(key);
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            if (Source.ContainsKey(key))
            {
                ScriptError error = new(ErrorLevel.Error, $"キー '{key}' の{ItemName}は既に存在します。");
                ErrorCollector.Report(error);
            }
            else
            {
                Source[key] = value;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear() => Source.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => Source.Contains(item);
        public bool ContainsKey(TKey key) => Source.ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => SourceAsCollection.CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Source.GetEnumerator();
        public bool Remove(TKey key) => Source.Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => SourceAsCollection.Remove(item);
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => Source.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
