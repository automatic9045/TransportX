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
    public abstract class BifoldDoorFactoryBase
    {
        private readonly DoorsBase Parent;

        private DoorPanel? HingedPanelValue = null;
        private DoorPanel? GuidePanelValue = null;
        private float PanelThicknessValue = 0.1f;

        private AnimationProfile OpenAnimationValue = new([(0, 0), (1, 1)], new PidGains(1, 0, 0), TimeSpan.FromSeconds(1));
        private AnimationProfile CloseAnimationValue = new([(0, 0), (1, 1)], new PidGains(1, 0, 0), TimeSpan.FromSeconds(1));
        private float Restitution0Value = 0;
        private float Restitution1Value = 0;

        private Signal<bool> DoorSwitchValue = new(false);

        public string Key { get; }

        public BifoldDoorCommand? BuiltDoor { get; private set; } = null;

        private protected BifoldDoorFactoryBase(DoorsBase parent, string key)
        {
            Parent = parent;
            Key = key;
        }

        public BifoldDoorFactoryBase HingedPanel(TransformedModel model, double width)
        {
            HingedPanelValue = new DoorPanel(model, (float)width);
            return this;
        }

        public BifoldDoorFactoryBase GuidePanel(TransformedModel model, double width)
        {
            GuidePanelValue = new DoorPanel(model, (float)width);
            return this;
        }

        public BifoldDoorFactoryBase PanelThickness(double thickness)
        {
            PanelThicknessValue = (float)thickness;
            return this;
        }

        public BifoldDoorFactoryBase OpenAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
        {
            OpenAnimationValue = new AnimationProfile(curvePoints, pidGains, duration);
            return this;
        }

        public BifoldDoorFactoryBase OpenAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => OpenAnimation(new PidGains((float)kP, (float)kI, (float)kD), TimeSpan.FromSeconds(durationSeconds), curvePoints);

        public BifoldDoorFactoryBase CloseAnimation(PidGains pidGains, TimeSpan duration, IReadOnlyCollection<CurvePoint> curvePoints)
        {
            CloseAnimationValue = new AnimationProfile(curvePoints, pidGains, duration);
            return this;
        }

        public BifoldDoorFactoryBase CloseAnimation(double kP, double kI, double kD, double durationSeconds, CurvePoint[] curvePoints)
            => CloseAnimation(new PidGains((float)kP, (float)kI, (float)kD), TimeSpan.FromSeconds(durationSeconds), curvePoints);

        public BifoldDoorFactoryBase Restitution(double restitution0, double restitution1)
        {
            Restitution0Value = (float)restitution0;
            Restitution1Value = (float)restitution1;
            return this;
        }

        public BifoldDoorFactoryBase DoorSwitch(Signal<bool> signal)
        {
            DoorSwitchValue = signal;
            return this;
        }

        public BifoldDoorFactoryBase DoorSwitch(string signalKey)
        {
            Signal<bool> signal = Parent.Signals.Bool(signalKey);
            return DoorSwitch(signal);
        }

        public BifoldDoorCommand Build()
        {
            if (BuiltDoor is not null)
            {
                ScriptError error = new(ErrorLevel.Error, "このドアは既にビルド済です。");
                Parent.ErrorCollector.Report(error);
                return BuiltDoor;
            }

            if (!HingedPanelValue.HasValue) return ReportAndCreateEmpty("ドアのヒンジパネルが指定されていません。");
            if (!GuidePanelValue.HasValue) return ReportAndCreateEmpty("ドアのガイドパネルが指定されていません。");

            DoorPanel hingedPanel = HingedPanelValue.Value;
            DoorPanel guidePanel = GuidePanelValue.Value;

            DoorAnimationProfile openProfile = CreateAnimationProfile(OpenAnimationValue);
            DoorAnimationProfile closeProfile = CreateAnimationProfile(CloseAnimationValue);
            DoorAnimator animator = new(openProfile, closeProfile, Restitution0Value, Restitution1Value);

            BifoldDoor door = new(hingedPanel.Model, guidePanel.Model, hingedPanel.Width, guidePanel.Width, PanelThicknessValue)
            {
                DoorSwitch = DoorSwitchValue,
                Animator = animator,
            };
            BuiltDoor = new BifoldDoorCommand(Key, door);
            Parent.Add(BuiltDoor);
            return BuiltDoor;


            BifoldDoorCommand ReportAndCreateEmpty(string message)
            {
                ScriptError error = new(ErrorLevel.Error, "このドアは既にビルド済です。");
                Parent.ErrorCollector.Report(error);
                return BifoldDoorCommand.Empty(Key);
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
