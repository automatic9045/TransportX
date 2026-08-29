using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Communication;
using TransportX.Diagnostics;
using TransportX.Mathematics;
using TransportX.Spatial;

using TransportX.Scripting;

using TransportX.Domains.Equipment.Doors;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public abstract class GlidingDoorFactoryBase<T> where T : GlidingDoorFactoryBase<T>
    {
        private readonly DoorsBase Parent;

        private TransformedModel? PanelModel = null;
        private Pose PanelOriginOffset = Pose.Identity;
        private float OpenAngle = 0;
        private float SlideDistance = 0;
        private float ArmLength = 0;
        private OpenDirection Direction = OpenDirection.Left;

        private AnimationProfile OpenAnimationValue = new([(0, 0), (1, 1)], new PidGains(1, 0, 0), TimeSpan.FromSeconds(1));
        private AnimationProfile CloseAnimationValue = new([(0, 0), (1, 1)], new PidGains(1, 0, 0), TimeSpan.FromSeconds(1));
        private float Restitution0Value = 0;
        private float Restitution1Value = 0;

        private Signal<bool> DoorSwitchValue = new(false);

        public string Key { get; }

        public GlidingDoorCommand? BuiltDoor { get; private set; } = null;

        private protected GlidingDoorFactoryBase(DoorsBase parent, string key)
        {
            Parent = parent;
            Key = key;
        }

        public T Panel(TransformedModel model, Pose originOffset, double openAngle, double slideDistance, double armLength)
        {
            PanelModel = model;
            PanelOriginOffset = originOffset;
            OpenAngle = MathHelper.ToRadians((float)openAngle);
            SlideDistance = (float)slideDistance;
            ArmLength = (float)armLength;
            return (T)this;
        }

        public T Panel(TransformedModel model, double x, double y, double z, double rotationX, double rotationY, double rotationZ,
            double openAngle, double slideDistance, double armLength)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return Panel(model, position.ToPose(), openAngle, slideDistance, armLength);
        }

        public T Panel(TransformedModel model, double x, double y, double z, double openAngle, double slideDistance, double armLength)
            => Panel(model, x, y, z, 0, 0, 0, openAngle, slideDistance, armLength);
        public T Panel(TransformedModel model, double openAngle, double slideDistance, double armLength)
            => Panel(model, 0, 0, 0, openAngle, slideDistance, armLength);

        public T OpenLeft()
        {
            Direction = OpenDirection.Left;
            return (T)this;
        }

        public T OpenRight()
        {
            Direction = OpenDirection.Right;
            return (T)this;
        }

        public T OpenAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
        {
            OpenAnimationValue = new AnimationProfile(curvePoints, pidGains, duration);
            return (T)this;
        }

        public T OpenAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => OpenAnimation(new PidGains((float)kP, (float)kI, (float)kD), TimeSpan.FromSeconds(durationSeconds), curvePoints);

        public T CloseAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
        {
            CloseAnimationValue = new AnimationProfile(curvePoints, pidGains, duration);
            return (T)this;
        }

        public T CloseAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => CloseAnimation(new PidGains((float)kP, (float)kI, (float)kD), TimeSpan.FromSeconds(durationSeconds), curvePoints);

        public T Restitution(double restitution0, double restitution1)
        {
            Restitution0Value = (float)restitution0;
            Restitution1Value = (float)restitution1;
            return (T)this;
        }

        public T DoorSwitch(Signal<bool> signal)
        {
            DoorSwitchValue = signal;
            return (T)this;
        }

        public T DoorSwitch(string signalKey)
        {
            Signal<bool> signal = Parent.Signals.Bool(signalKey);
            return DoorSwitch(signal);
        }

        public GlidingDoorCommand Build()
        {
            if (BuiltDoor is not null)
            {
                ScriptError error = new(ErrorLevel.Error, "このドアは既にビルド済です。");
                Parent.ErrorCollector.Report(error);
                return BuiltDoor;
            }

            if (PanelModel is null) return ReportAndCreateEmpty("ドアのパネルが指定されていません。");

            DoorAnimationProfile openProfile = CreateAnimationProfile(OpenAnimationValue);
            DoorAnimationProfile closeProfile = CreateAnimationProfile(CloseAnimationValue);
            DoorAnimator animator = new(openProfile, closeProfile, Restitution0Value, Restitution1Value);

            GlidingDoor door = new(PanelModel, PanelOriginOffset, OpenAngle, SlideDistance, ArmLength, Direction)
            {
                DoorSwitch = DoorSwitchValue,
                Animator = animator,
            };
            BuiltDoor = new GlidingDoorCommand(Key, door);
            Parent.Add(BuiltDoor);
            return BuiltDoor;


            GlidingDoorCommand ReportAndCreateEmpty(string message)
            {
                ScriptError error = new(ErrorLevel.Error, message);
                Parent.ErrorCollector.Report(error);
                return GlidingDoorCommand.Empty(Key);
            }

            DoorAnimationProfile CreateAnimationProfile(in AnimationProfile profile)
            {
                Curve curve = new(profile.CurvePoints);
                PidController pid = new()
                {
                    K = profile.PidGains,
                };

                return new DoorAnimationProfile(curve, pid, profile.Duration);
            }
        }


        private readonly record struct AnimationProfile(IReadOnlyCollection<CurvePoint> CurvePoints, PidGains PidGains, TimeSpan Duration);
    }
}
