using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;

using TransportX.Scripting.Commands;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Sounds
    {
        private readonly ScriptAvatar Avatar;
        private readonly SoundsInternal Internal;

        private int DefaultKeyIndex = 0;

        internal Sounds(ScriptAvatar avatar)
        {
            Avatar = avatar;
            Internal = new SoundsInternal(Avatar.DXHost, Avatar.Sounds, Avatar.ErrorCollector);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ISoundBundle LoadList(string key, string path)
        {
            return Internal.LoadList(key, path, Avatar.BaseDirectory);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ISoundBundle LoadList(string listPath)
        {
            string key;
            do
            {
                key = FormattableString.Invariant($"Avatar_{DefaultKeyIndex++}");
            }
            while (Avatar.Sounds.Bundles.Contains(key));

            return Internal.LoadList(key, listPath, Avatar.BaseDirectory);
        }
    }
}
