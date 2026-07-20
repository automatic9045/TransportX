using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;
using TransportX.Mathematics;
using TransportX.Spatial;

using TransportX.Scripting.Avatars.Commands;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class AvatarSlidingDoorFactory : SlidingDoorFactoryBase
    {
        private readonly AvatarDoors Parent;

        public AvatarSlidingDoorFactory(AvatarDoors parent, string key) : base(parent, key)
        {
            Parent = parent;
        }

        public AvatarSlidingDoorFactory Panel(Part part, double width)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part) ? Panel(part.Model, width) : this;

        public AvatarSlidingDoorFactory Panel(string partKey, double width)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part) ? Panel(part, width) : this;

        public new AvatarSlidingDoorFactory Panel(TransformedModel model, double width) => (AvatarSlidingDoorFactory)base.Panel(model, width);
        public new AvatarSlidingDoorFactory OpenAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
            => (AvatarSlidingDoorFactory)base.OpenAnimation(pidGains, duration, curvePoints);
        public new AvatarSlidingDoorFactory OpenAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => (AvatarSlidingDoorFactory)base.OpenAnimation(kP, kI, kD, durationSeconds, curvePoints);
        public new AvatarSlidingDoorFactory CloseAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
            => (AvatarSlidingDoorFactory)base.CloseAnimation(pidGains, duration, curvePoints);
        public new AvatarSlidingDoorFactory CloseAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => (AvatarSlidingDoorFactory)base.CloseAnimation(kP, kI, kD, durationSeconds, curvePoints);
        public new AvatarSlidingDoorFactory Restitution(double restitution0, double restitution1) => (AvatarSlidingDoorFactory)base.Restitution(restitution0, restitution1);
        public new AvatarSlidingDoorFactory DoorSwitch(Signal<bool> signal) => (AvatarSlidingDoorFactory)base.DoorSwitch(signal);
        public new AvatarSlidingDoorFactory DoorSwitch(string signalKey) => (AvatarSlidingDoorFactory)base.DoorSwitch(signalKey);
    }
}
