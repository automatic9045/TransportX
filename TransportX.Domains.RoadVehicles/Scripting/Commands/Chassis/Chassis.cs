using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Scripting;
using TransportX.Scripting.Avatars;
using TransportX.Scripting.Collections;

using TransportX.Domains.RoadVehicles.Chassis;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Chassis
{
    public class Chassis : IAvatarInstantiable<Chassis>, IComponentCommand
    {
        private readonly ScriptAvatar Avatar;

        public ChassisComponent Source { get; }
        IComponent IComponentCommand.Source => Source;

        private readonly ScriptKeyedList<string, AxleCommand> AxlesKey;
        public IReadOnlyScriptKeyedList<string, AxleCommand> Axles => AxlesKey;

        public Chassis(ScriptAvatar avatar)
        {
            Avatar = avatar;

            Source = new ChassisComponent();
            AxlesKey = new ScriptKeyedList<string, AxleCommand>(
                axle => axle.Key, Avatar.ErrorCollector, "車軸", key => AxleCommand.InvalidEmpty(Avatar, key));
        }

        public static Chassis Create(ScriptAvatar avatar) => new(avatar);

        public AxleFactory AddAxle(string key)
        {
            AxleFactory factory = new(Avatar, key);
            return factory;
        }

        public void AddAxle(AxleCommand axleCommand)
        {
            AxlesKey.Add(axleCommand);
            Source.Axles.Add(axleCommand.Axle);
        }

        public CoilRigidSuspensionFactory AddCoilRigidSuspension(string keyBase, string bodyPartKey, string axleKey)
        {
            CoilRigidSuspensionFactory factory = new(Avatar, keyBase, bodyPartKey, axleKey);
            return factory;
        }
    }
}
