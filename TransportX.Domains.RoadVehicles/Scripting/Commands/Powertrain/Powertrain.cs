using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;

using TransportX.Domains.RoadVehicles.Powertrain;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain
{
    public class Powertrain : IAvatarInstantiable<Powertrain>, IComponentCommand
    {
        private readonly ScriptAvatar Avatar;

        public PowertrainComponent Source { get; }
        IComponent IComponentCommand.Source => Source;

        public Modules Modules { get; }
        public Controllers Controllers { get; }

        public Powertrain(ScriptAvatar avatar)
        {
            Avatar = avatar;

            Source = new PowertrainComponent();
            Source.Started += Build;

            Modules = new Modules(avatar, Source);
            Controllers = new Controllers(avatar, Source);
        }

        public static Powertrain Create(ScriptAvatar avatar) => new(avatar);

        public void Build()
        {
            Modules.Build();
            Controllers.Build();
        }
    }
}
