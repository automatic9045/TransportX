using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Collections
{
    public interface IReadOnlyKeyedList<TKey, TValue> : IReadOnlyList<TValue>
    {
        TValue this[TKey key] { get; }

        bool Contains(TKey key);
        bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue item);
    }
}
