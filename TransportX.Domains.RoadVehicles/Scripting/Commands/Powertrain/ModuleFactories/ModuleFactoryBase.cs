using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ModuleFactories
{
    public abstract class ModuleFactoryBase
    {
        protected readonly ScriptAvatar Avatar;

        public string Key { get; }
        public ModuleCommand? BuiltModule { get; protected set; } = null;

        protected ModuleFactoryBase(ScriptAvatar avatar, string key)
        {
            Avatar = avatar;
            Key = key;
        }

        internal static ModuleFactoryBase InvalidEmpty(ScriptAvatar avatar, string key)
        {
            return new EmptyFactory(avatar, key);
        }

        protected abstract IModule OnBuild();

        internal ModuleCommand Build()
        {
            if (BuiltModule is not null) throw new InvalidOperationException();

            IModule module = OnBuild();
            BuiltModule = Avatar.Commander.Component<Powertrain>().Modules.Add(Key, module);
            return BuiltModule;
        }


        private class EmptyFactory : ModuleFactoryBase
        {
            public EmptyFactory(ScriptAvatar avatar, string key) : base(avatar, key)
            {
            }

            protected override IModule OnBuild()
            {
                return IModule.Empty();
            }
        }
    }
}
