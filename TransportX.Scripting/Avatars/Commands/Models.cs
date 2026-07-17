using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;
using TransportX.Scripting.Commands;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Models
    {
        private readonly ScriptAvatar Avatar;
        private readonly ModelsInternal Internal;

        private int DefaultKeyIndex = 0;

        internal Models(ScriptAvatar avatar)
        {
            Avatar = avatar;
            Internal = new ModelsInternal(Avatar.DXHost, Avatar.PhysicsHost, Avatar.Models, Avatar.ErrorCollector);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IModelBundle LoadList(string key, string path)
        {
            return Internal.LoadList(key, path, Avatar.BaseDirectory);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IModelBundle LoadList(string listPath)
        {
            string key;
            do
            {
                key = FormattableString.Invariant($"Avatar_{DefaultKeyIndex++}");
            }
            while (Avatar.Models.Bundles.Contains(key));

            return Internal.LoadList(key, listPath, Avatar.BaseDirectory);
        }
    }
}
