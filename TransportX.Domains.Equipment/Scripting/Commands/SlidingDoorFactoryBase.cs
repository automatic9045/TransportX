using System;
using System.Collections.Generic;
using System.Linq;
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
    public abstract class SlidingDoorFactoryBase
    {
        private readonly DoorsBase Parent;

        private DoorPanel? PanelValue = null;

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

        public SlidingDoorFactoryBase Panel(TransformedModel model, double width)
        {
            PanelValue = new DoorPanel(model, (float)width);
            return this;
        }

        public SlidingDoorFactoryBase OpenAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
        {
            OpenAnimationValue = new AnimationProfile(curvePoints, pidGains, duration);
            return this;
        }

        public SlidingDoorFactoryBase OpenAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => OpenAnimation(new PidGains((float)kP, (float)kI, (float)kD), TimeSpan.FromSeconds(durationSeconds), curvePoints);

        public SlidingDoorFactoryBase CloseAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
        {
            CloseAnimationValue = new AnimationProfile(curvePoints, pidGains, duration);
            return this;
        }

        public SlidingDoorFactoryBase CloseAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => CloseAnimation(new PidGains((float)kP, (float)kI, (float)kD), TimeSpan.FromSeconds(durationSeconds), curvePoints);

        public SlidingDoorFactoryBase Restitution(double restitution0, double restitution1)
        {
            Restitution0Value = (float)restitution0;
            Restitution1Value = (float)restitution1;
            return this;
        }

        public SlidingDoorFactoryBase DoorSwitch(Signal<bool> signal)
        {
            DoorSwitchValue = signal;
            return this;
        }

        public SlidingDoorFactoryBase DoorSwitch(string signalKey)
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

            if (!PanelValue.HasValue) return ReportAndCreateEmpty("ドアのパネルが指定されていません。");

            DoorPanel Panel = PanelValue.Value;

            DoorAnimationProfile openProfile = CreateAnimationProfile(OpenAnimationValue);
            DoorAnimationProfile closeProfile = CreateAnimationProfile(CloseAnimationValue);
            DoorAnimator animator = new(openProfile, closeProfile, Restitution0Value, Restitution1Value);

            SlidingDoor door = new(Panel.Model, Panel.Width)
            {
                DoorSwitch = DoorSwitchValue,
                Animator = animator,
            };
            BuiltDoor = new SlidingDoorCommand(Key, door);
            Parent.Add(BuiltDoor);
            return BuiltDoor;


            SlidingDoorCommand ReportAndCreateEmpty(string message)
            {
                ScriptError error = new(ErrorLevel.Error, "このドアは既にビルド済です。");
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


        private readonly record struct DoorPanel(TransformedModel Model, float Width);
        private readonly record struct AnimationProfile(IReadOnlyCollection<CurvePoint> CurvePoints, PidGains PidGains, TimeSpan Duration);
    }
}
