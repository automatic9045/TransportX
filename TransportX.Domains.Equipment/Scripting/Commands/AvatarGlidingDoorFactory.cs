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
    public class AvatarGlidingDoorFactory : GlidingDoorFactoryBase
    {
        private readonly AvatarDoors Parent;

        public AvatarGlidingDoorFactory(AvatarDoors parent, string key) : base(parent, key)
        {
            Parent = parent;
        }

        public AvatarGlidingDoorFactory Panel(Part part,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double openAngle, double slideDistance, double armLength)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part)
            ? Panel(part.Model, x, y, z, rotationX, rotationY, rotationZ, openAngle, slideDistance, armLength) : this;
        public AvatarGlidingDoorFactory Panel(Part part, double x, double y, double z, double openAngle, double slideDistance, double armLength)
            => Panel(part, x, y, z, 0, 0, 0, openAngle, slideDistance, armLength);
        public AvatarGlidingDoorFactory Panel(Part part, double openAngle, double slideDistance, double armLength)
            => Panel(part, 0, 0, 0, openAngle, slideDistance, armLength);

        public AvatarGlidingDoorFactory Panel(string partKey,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double openAngle, double slideDistance, double armLength)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part)
            ? Panel(part, x, y, z, rotationX, rotationY, rotationZ, openAngle, slideDistance, armLength) : this;
        public AvatarGlidingDoorFactory Panel(string partKey, double x, double y, double z, double openAngle, double slideDistance, double armLength)
            => Panel(partKey, x, y, z, 0, 0, 0, openAngle, slideDistance, armLength);
        public AvatarGlidingDoorFactory Panel(string partKey, double openAngle, double slideDistance, double armLength)
            => Panel(partKey, 0, 0, 0, openAngle, slideDistance, armLength);

        public new AvatarGlidingDoorFactory Panel(TransformedModel model, Pose originOffset, double openAngle, double slideDistance, double armLength)
            => (AvatarGlidingDoorFactory)base.Panel(model, originOffset, openAngle, slideDistance, armLength);
        public new AvatarGlidingDoorFactory Panel(TransformedModel model,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double openAngle, double slideDistance, double armLength)
            => (AvatarGlidingDoorFactory)base.Panel(model, x, y, z, rotationX, rotationY, rotationZ, openAngle, slideDistance, armLength);
        public new AvatarGlidingDoorFactory Panel(TransformedModel model, double x, double y, double z, double openAngle, double slideDistance, double armLength)
            => (AvatarGlidingDoorFactory)base.Panel(model, x, y, z, openAngle, slideDistance, armLength);
        public new AvatarGlidingDoorFactory Panel(TransformedModel model, double openAngle, double slideDistance, double armLength)
            => (AvatarGlidingDoorFactory)base.Panel(model, openAngle, slideDistance, armLength);
        public new AvatarGlidingDoorFactory OpenLeft() => (AvatarGlidingDoorFactory)base.OpenLeft();
        public new AvatarGlidingDoorFactory OpenRight() => (AvatarGlidingDoorFactory)base.OpenRight();
        public new AvatarGlidingDoorFactory OpenAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
            => (AvatarGlidingDoorFactory)base.OpenAnimation(pidGains, duration, curvePoints);
        public new AvatarGlidingDoorFactory OpenAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => (AvatarGlidingDoorFactory)base.OpenAnimation(kP, kI, kD, durationSeconds, curvePoints);
        public new AvatarGlidingDoorFactory CloseAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
            => (AvatarGlidingDoorFactory)base.CloseAnimation(pidGains, duration, curvePoints);
        public new AvatarGlidingDoorFactory CloseAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => (AvatarGlidingDoorFactory)base.CloseAnimation(kP, kI, kD, durationSeconds, curvePoints);
        public new AvatarGlidingDoorFactory Restitution(double restitution0, double restitution1) => (AvatarGlidingDoorFactory)base.Restitution(restitution0, restitution1);
        public new AvatarGlidingDoorFactory DoorSwitch(Signal<bool> signal) => (AvatarGlidingDoorFactory)base.DoorSwitch(signal);
        public new AvatarGlidingDoorFactory DoorSwitch(string signalKey) => (AvatarGlidingDoorFactory)base.DoorSwitch(signalKey);
    }
}
