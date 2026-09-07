using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

namespace TransportX.Input.Configuration
{
    public class KeyboardAxisBinding
    {
        public string Key { get; }
        public IReadOnlyList<Key> Keys { get; }

        public KeyboardAxisBinding(string key, IReadOnlyList<Key> keys)
        {
            Key = key;
            Keys = keys;
        }
    }
}
