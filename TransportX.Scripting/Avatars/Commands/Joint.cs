using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using TransportX.Components;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Joint
    {
        private readonly ScriptAvatar Avatar;

        public string Key { get; }
        public ConstraintHandle Handle { get; }

        public IComponentCollection<IComponent> Components { get; } = new ComponentCollection<IComponent>();

        internal Joint(ScriptAvatar avatar, string key, ConstraintHandle handle)
        {
            Avatar = avatar;

            Key = key;
            Handle = handle;
        }

        public static Joint Empty(ScriptAvatar avatar, string key)
        {
            return new Joint(avatar, key, new ConstraintHandle(-1));
        }

        internal void RegisterComponents()
        {
            Avatar.ComponentEngine.Register(Components);
        }
    }
}
