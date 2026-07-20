using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;
using TransportX.Scripting.Avatars.Commands;
using TransportX.Scripting.Collections;
using TransportX.Scripting.Commands;
using TransportX.Scripting.Worlds;

using TransportX.Domains.Equipment.Doors;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class AvatarDoors : DoorsBase, IAvatarInstantiable<AvatarDoors>
    {
        internal ScriptAvatar Avatar { get; }

        public AvatarDoors(ScriptAvatar avatar) : base(avatar.Commander.Signals, avatar.ErrorCollector)
        {
            Avatar = avatar;
        }

        public static AvatarDoors Create(ScriptAvatar avatar) => new(avatar);

        public AvatarBifoldDoorFactory AddBiford(string key)
        {
            AvatarBifoldDoorFactory factory = new(this, key);
            return factory;
        }

        public AvatarSlidingDoorFactory AddSliding(string key)
        {
            AvatarSlidingDoorFactory factory = new(this, key);
            return factory;
        }
    }
}
