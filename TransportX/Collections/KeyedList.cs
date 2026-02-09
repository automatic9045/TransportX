using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Collections
{
    public class KeyedList<TKey, TValue> : KeyedCollection<TKey, TValue>, IReadOnlyKeyedList<TKey, TValue> where TKey : notnull
    {
        private readonly Converter<TValue, TKey> ItemKeySelector;

        public KeyedList(Converter<TValue, TKey> itemKeySelector, IEnumerable<TValue> collection, IEqualityComparer<TKey>? comparer) : base(comparer, 0)
        {
            ItemKeySelector = itemKeySelector;
            foreach (var item in collection) Add(item);
        }

        public KeyedList(Converter<TValue, TKey> itemKeySelector, IEnumerable<TValue> collection) : this(itemKeySelector, collection, null)
        {
        }

        public KeyedList(Converter<TValue, TKey> itemKeySelector, IEqualityComparer<TKey>? comparer) : this(itemKeySelector, [], comparer)
        {
        }

        public KeyedList(Converter<TValue, TKey> itemKeySelector) : this(itemKeySelector, [], null)
        {
        }

        protected override TKey GetKeyForItem(TValue item) => ItemKeySelector(item);
    }
}
