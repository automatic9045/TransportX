using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain
{
    public class ModuleCommand
    {
        private readonly ScriptAvatar Avatar;

        public string Key { get; }
        public IModule Module { get; }

        public ModuleCommand(ScriptAvatar avatar, string key, IModule module)
        {
            Avatar = avatar;

            Key = key;
            Module = module;
        }

        internal static ModuleCommand InvalidEmpty(ScriptAvatar avatar, string key)
        {
            return new ModuleCommand(avatar, key, null!);
        }
    }
}
