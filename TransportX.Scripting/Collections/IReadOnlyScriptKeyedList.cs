using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;

namespace TransportX.Scripting.Collections
{
    public interface IReadOnlyScriptKeyedList<TKey, TValue> : IReadOnlyKeyedList<TKey, TValue> where TKey : notnull
    {
        string ItemName { get; }

        bool GetValue(TKey key, out TValue value);
    }
}
