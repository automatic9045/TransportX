using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;

using TransportX.Scripting.Commands;

namespace TransportX.Scripting.Worlds.Commands
{
    public class Sounds
    {
        private readonly ScriptWorld World;
        private readonly SoundsInternal Internal;

        private int DefaultKeyIndex = 0;

        internal Sounds(ScriptWorld world)
        {
            World = world;
            Internal = new SoundsInternal(World.DXHost, World.Sounds, World.ErrorCollector);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ISoundBundle LoadList(string key, string path)
        {
            return Internal.LoadList(key, path, World.BaseDirectory);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ISoundBundle LoadList(string listPath)
        {
            string key;
            do
            {
                key = DefaultKeyIndex++.ToString(CultureInfo.InvariantCulture);
            }
            while (World.Sounds.Bundles.Contains(key));

            return Internal.LoadList(key, listPath, World.BaseDirectory);
        }
    }
}
