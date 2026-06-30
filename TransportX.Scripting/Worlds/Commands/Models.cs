using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;
using TransportX.Scripting.Commands;

namespace TransportX.Scripting.Worlds.Commands
{
    public sealed class Models
    {
        private readonly ScriptWorld World;
        private readonly ModelsInternal Internal;

        private int DefaultKeyIndex = 0;

        internal Models(ScriptWorld world)
        {
            World = world;
            Internal = new ModelsInternal(World.DXHost, World.PhysicsHost, World.Models, World.ErrorCollector);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IModelBundle LoadList(string key, string path)
        {
            return Internal.LoadList(key, path, World.BaseDirectory);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IModelBundle LoadList(string listPath)
        {
            string key;
            do
            {
                key = DefaultKeyIndex++.ToString(CultureInfo.InvariantCulture);
            }
            while (World.Models.Bundles.Contains(key));

            return Internal.LoadList(key, listPath, World.BaseDirectory);
        }
    }
}
