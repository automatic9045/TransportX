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
    public class AvatarBifoldDoorFactory : BifoldDoorFactoryBase
    {
        private readonly AvatarDoors Parent;

        public AvatarBifoldDoorFactory(AvatarDoors parent, string key) : base(parent, key)
        {
            Parent = parent;
        }

        public AvatarBifoldDoorFactory HingedPanel(string partKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part) ? HingedPanel(part, x, y, z, rotationX, rotationY, rotationZ, width) : this;
        public AvatarBifoldDoorFactory GuidePanel(string partKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part) ? GuidePanel(part, x, y, z, rotationX, rotationY, rotationZ, width) : this;

        public AvatarBifoldDoorFactory HingedPanel(Part part, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part) ? HingedPanel(part.Model, x, y, z, rotationX, rotationY, rotationZ, width) : this;
        public AvatarBifoldDoorFactory GuidePanel(Part part, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part) ? GuidePanel(part.Model, x, y, z, rotationX, rotationY, rotationZ, width) : this;

        public AvatarBifoldDoorFactory HingedPanel(string partKey, double x, double y, double z, double width)
            => HingedPanel(partKey, x, y, z, 0, 0, 0, width);
        public AvatarBifoldDoorFactory HingedPanel(string partKey, double width)
            => HingedPanel(partKey, 0, 0, 0, width);
        public AvatarBifoldDoorFactory GuidePanel(string partKey, double x, double y, double z, double width)
            => GuidePanel(partKey, x, y, z, 0, 0, 0, width);
        public AvatarBifoldDoorFactory GuidePanel(string partKey, double width)
            => GuidePanel(partKey, 0, 0, 0, width);

        public AvatarBifoldDoorFactory HingedPanel(Part part, double x, double y, double z, double width)
            => HingedPanel(part, x, y, z, 0, 0, 0, width);
        public AvatarBifoldDoorFactory HingedPanel(Part part, double width)
            => HingedPanel(part, 0, 0, 0, width);
        public AvatarBifoldDoorFactory GuidePanel(Part part, double x, double y, double z, double width)
            => GuidePanel(part, x, y, z, 0, 0, 0, width);
        public AvatarBifoldDoorFactory GuidePanel(Part part, double width)
            => GuidePanel(part, 0, 0, 0, width);

        public new AvatarBifoldDoorFactory HingedPanel(TransformedModel model, Pose originOffset, double width)
            => (AvatarBifoldDoorFactory)base.HingedPanel(model, originOffset, width);
        public new AvatarBifoldDoorFactory HingedPanel(TransformedModel model, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => (AvatarBifoldDoorFactory)base.HingedPanel(model, x, y, z, rotationX, rotationY, rotationZ, width);
        public new AvatarBifoldDoorFactory HingedPanel(TransformedModel model, double x, double y, double z, double width)
            => (AvatarBifoldDoorFactory)base.HingedPanel(model, x, y, z, width);
        public new AvatarBifoldDoorFactory HingedPanel(TransformedModel model, double width)
            => (AvatarBifoldDoorFactory)base.HingedPanel(model, width);
        public new AvatarBifoldDoorFactory GuidePanel(TransformedModel model, Pose originOffset, double width)
            => (AvatarBifoldDoorFactory)base.GuidePanel(model, originOffset, width);
        public new AvatarBifoldDoorFactory GuidePanel(TransformedModel model, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => (AvatarBifoldDoorFactory)base.GuidePanel(model, x, y, z, rotationX, rotationY, rotationZ, width);
        public new AvatarBifoldDoorFactory GuidePanel(TransformedModel model, double x, double y, double z, double width)
            => (AvatarBifoldDoorFactory)base.GuidePanel(model, x, y, z, width);
        public new AvatarBifoldDoorFactory GuidePanel(TransformedModel model, double width)
            => (AvatarBifoldDoorFactory)base.GuidePanel(model, width);
        public new AvatarBifoldDoorFactory PanelThickness(double thickness) => (AvatarBifoldDoorFactory)base.PanelThickness(thickness);
        public new AvatarBifoldDoorFactory OpenLeft() => (AvatarBifoldDoorFactory)base.OpenLeft();
        public new AvatarBifoldDoorFactory OpenRight() => (AvatarBifoldDoorFactory)base.OpenRight();
        public new AvatarBifoldDoorFactory OpenAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
            => (AvatarBifoldDoorFactory)base.OpenAnimation(pidGains, duration, curvePoints);
        public new AvatarBifoldDoorFactory OpenAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => (AvatarBifoldDoorFactory)base.OpenAnimation(kP, kI, kD, durationSeconds, curvePoints);
        public new AvatarBifoldDoorFactory CloseAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
            => (AvatarBifoldDoorFactory)base.CloseAnimation(pidGains, duration, curvePoints);
        public new AvatarBifoldDoorFactory CloseAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => (AvatarBifoldDoorFactory)base.CloseAnimation(kP, kI, kD, durationSeconds, curvePoints);
        public new AvatarBifoldDoorFactory Restitution(double restitution0, double restitution1) => (AvatarBifoldDoorFactory)base.Restitution(restitution0, restitution1);
        public new AvatarBifoldDoorFactory DoorSwitch(Signal<bool> signal) => (AvatarBifoldDoorFactory)base.DoorSwitch(signal);
        public new AvatarBifoldDoorFactory DoorSwitch(string signalKey) => (AvatarBifoldDoorFactory)base.DoorSwitch(signalKey);
    }
}
