using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;
using TransportX.Diagnostics;
using TransportX.Mathematics;
using TransportX.Spatial;

using TransportX.Scripting;

using TransportX.Domains.Equipment.Doors;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public abstract class SlidingDoorFactoryBase<T> where T : SlidingDoorFactoryBase<T>
    {
        private readonly DoorsBase Parent;

        private TransformedModel? PanelModel = null;
        private Quaternion PanelOriginOffset = Quaternion.Identity;
        private float PanelWidth = 1;
        private OpenDirection Direction = OpenDirection.Left;

        private AnimationProfile OpenAnimationValue = new([(0, 0), (1, 1)], new PidGains(1, 0, 0), TimeSpan.FromSeconds(1));
        private AnimationProfile CloseAnimationValue = new([(0, 0), (1, 1)], new PidGains(1, 0, 0), TimeSpan.FromSeconds(1));
        private float Restitution0Value = 0;
        private float Restitution1Value = 0;

        private Signal<bool> DoorSwitchValue = new(false);

        public string Key { get; }

        public SlidingDoorCommand? BuiltDoor { get; private set; } = null;

        private protected SlidingDoorFactoryBase(DoorsBase parent, string key)
        {
            Parent = parent;
            Key = key;
        }

        public T Panel(TransformedModel model, Quaternion originOffset, double width)
        {
            PanelModel = model;
            PanelOriginOffset = originOffset;
            PanelWidth = (float)width;
            return (T)this;
        }

        public T Panel(TransformedModel model, double rotationX, double rotationY, double rotationZ, double width)
        {
            SixDoF position = SixDoF.FromDegrees(0, 0, 0, (float)rotationX, (float)rotationY, (float)rotationZ);
            return Panel(model, position.ToQuaternion(), width);
        }

        public T Panel(TransformedModel model, double width)
            => Panel(model, Quaternion.Identity, width);

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

        public SlidingDoorCommand Build()
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

            SlidingDoor door = new(PanelModel, PanelOriginOffset, PanelWidth * (int)Direction)
            {
                DoorSwitch = DoorSwitchValue,
                Animator = animator,
            };
            BuiltDoor = new SlidingDoorCommand(Key, door);
            Parent.Add(BuiltDoor);
            return BuiltDoor;


            SlidingDoorCommand ReportAndCreateEmpty(string message)
            {
                ScriptError error = new(ErrorLevel.Error, message);
                Parent.ErrorCollector.Report(error);
                return SlidingDoorCommand.Empty(Key);
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
