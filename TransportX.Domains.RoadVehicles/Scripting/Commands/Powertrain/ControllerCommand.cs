using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain
{
    public class ControllerCommand
    {
        private readonly ScriptAvatar Avatar;

        public string Key { get; }
        public IController Controller { get; }

        public ControllerCommand(ScriptAvatar avatar, string key, IController controller)
        {
            Avatar = avatar;

            Key = key;
            Controller = controller;
        }

        internal static ControllerCommand InvalidEmpty(ScriptAvatar avatar, string key)
        {
            return new ControllerCommand(avatar, key, null!);
        }
    }
}
