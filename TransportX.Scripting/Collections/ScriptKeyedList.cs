using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Diagnostics;

namespace TransportX.Scripting.Collections
{
    public class ScriptKeyedList<TKey, TValue> : KeyedList<TKey, TValue>, IReadOnlyKeyedList<TKey, TValue> where TKey : notnull
    {
        private readonly IErrorCollector ErrorCollector;
        private readonly string ItemName;
        private readonly Func<TKey, TValue> DefaultFactory;

        public new TValue this[TKey key]
        {
            get
            {
                GetValue(key, out TValue value);
                return value;
            }
        }

        TValue IReadOnlyKeyedList<TKey, TValue>.this[TKey key] => this[key];

        public ScriptKeyedList(Converter<TValue, TKey> itemKeySelector, IEnumerable<TValue> collection, IEqualityComparer<TKey>? comparer,
            IErrorCollector errorCollector, string itemName, Func<TKey, TValue> defaultFactory)
            : base(itemKeySelector, collection, comparer)
        {
            ErrorCollector = errorCollector;
            ItemName = itemName;
            DefaultFactory = defaultFactory;
        }

        public ScriptKeyedList(Converter<TValue, TKey> itemKeySelector, IEnumerable<TValue> collection,
            IErrorCollector errorCollector, string itemName, Func<TKey, TValue> defaultFactory)
            : this(itemKeySelector, collection, null, errorCollector, itemName, defaultFactory)
        {
        }

        public ScriptKeyedList(Converter<TValue, TKey> itemKeySelector, IEqualityComparer<TKey>? comparer,
            IErrorCollector errorCollector, string itemName, Func<TKey, TValue> defaultFactory)
            : this(itemKeySelector, [], comparer, errorCollector, itemName, defaultFactory)
        {
        }

        public ScriptKeyedList(Converter<TValue, TKey> itemKeySelector, IErrorCollector errorCollector, string itemName, Func<TKey, TValue> defaultFactory)
            : this(itemKeySelector, [], errorCollector, itemName, defaultFactory)
        {
        }

        public bool GetValue(TKey key, out TValue value)
        {
            if (TryGetValue(key, out TValue? sourceValue))
            {
                value = sourceValue;
                return true;
            }

            ScriptError error = new(ErrorLevel.Error, $"{ItemName} '{key}' が見つかりません。");
            ErrorCollector.Report(error);

            value = DefaultFactory(key);
            return false;
        }
    }
}
