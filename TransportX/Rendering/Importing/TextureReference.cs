using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Importing
{
    internal readonly struct TextureReference
    {
        public readonly TextureType Type { get; }
        public readonly string Key { get; }

        public TextureReference(TextureType type, string key)
        {
            Type = type;
            Key = key;
        }

        internal enum TextureType
        {
            File,
            Embedded,
        }
    }
}
