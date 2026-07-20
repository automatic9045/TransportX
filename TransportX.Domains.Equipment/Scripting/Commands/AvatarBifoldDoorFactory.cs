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

        public AvatarBifoldDoorFactory HingedPanel(Part part, double width)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part) ? HingedPanel(part.Model, width) : this;

        public AvatarBifoldDoorFactory GuidePanel(Part part, double width)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part) ? GuidePanel(part.Model, width) : this;

        public AvatarBifoldDoorFactory HingedPanel(string partKey, double width)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part) ? HingedPanel(part, width) : this;

        public AvatarBifoldDoorFactory GuidePanel(string partKey, double width)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part) ? GuidePanel(part, width) : this;

        public new AvatarBifoldDoorFactory HingedPanel(TransformedModel model, double width) => (AvatarBifoldDoorFactory)base.HingedPanel(model, width);
        public new AvatarBifoldDoorFactory GuidePanel(TransformedModel model, double width) => (AvatarBifoldDoorFactory)base.GuidePanel(model, width);
        public new AvatarBifoldDoorFactory PanelThickness(double thickness) => (AvatarBifoldDoorFactory)base.PanelThickness(thickness);
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
