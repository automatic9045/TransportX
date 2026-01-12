using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scripting
{
    public interface IReadOnlyScriptDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        bool GetValue(TKey key, out TValue value);
    }
}
