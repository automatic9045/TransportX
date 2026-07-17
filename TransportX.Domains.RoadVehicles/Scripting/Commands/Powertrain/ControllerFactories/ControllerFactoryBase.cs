using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ControllerFactories
{
    public abstract class ControllerFactoryBase
    {
        protected readonly ScriptAvatar Avatar;

        public string Key { get; }
        public ControllerCommand? BuiltController { get; protected set; } = null;

        protected ControllerFactoryBase(ScriptAvatar avatar, string key)
        {
            Avatar = avatar;
            Key = key;
        }

        internal static ControllerFactoryBase InvalidEmpty(ScriptAvatar avatar, string key)
        {
            return new EmptyFactory(avatar, key);
        }

        protected abstract IController OnBuild();

        internal ControllerCommand Build()
        {
            if (BuiltController is not null) throw new InvalidOperationException();

            IController controller = OnBuild();
            BuiltController = Avatar.Commander.Component<Powertrain>().Controllers.Add(Key, controller);
            return BuiltController;
        }


        private class EmptyFactory : ControllerFactoryBase
        {
            public EmptyFactory(ScriptAvatar avatar, string key) : base(avatar, key)
            {
            }

            protected override IController OnBuild()
            {
                return IController.Empty();
            }
        }
    }
}
