using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;

using TransportX.Domains.RoadVehicles.Lighting;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Lighting
{
    public class Lighting : IAvatarInstantiable<Lighting>, IComponentCommand
    {
        internal ScriptAvatar Avatar { get; }

        public LightingComponent Source { get; }
        IComponent IComponentCommand.Source => Source;

        public Blinkers Blinkers { get; }

        public Lighting(ScriptAvatar avatar)
        {
            Avatar = avatar;
            Source = new LightingComponent();

            Blinkers = new Blinkers(this);
        }

        public static Lighting Create(ScriptAvatar avatar) => new(avatar);
    }
}
