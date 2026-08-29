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
    public abstract class BifoldDoorFactoryBase<T> where T : BifoldDoorFactoryBase<T>
    {
        private readonly DoorsBase Parent;

        private BifoldDoor.Panel? HingedPanelValue = null;
        private BifoldDoor.Panel? GuidePanelValue = null;
        private float PanelThicknessValue = 0.1f;
        private OpenDirection DirectionValue = OpenDirection.Left;

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

        public T HingedPanel(TransformedModel model, Pose originOffset, double width)
        {
            HingedPanelValue = new BifoldDoor.Panel(model, originOffset, (float)width);
            return (T)this;
        }

        public T HingedPanel(TransformedModel model, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return HingedPanel(model, position.ToPose(), width);
        }

        public T HingedPanel(TransformedModel model, double x, double y, double z, double width)
            => HingedPanel(model, x, y, z, 0, 0, 0, width);
        public T HingedPanel(TransformedModel model, double width)
            => HingedPanel(model, 0, 0, 0, width);

        public T GuidePanel(TransformedModel model, Pose originOffset, double width)
        {
            GuidePanelValue = new BifoldDoor.Panel(model, originOffset, (float)width);
            return (T)this;
        }

        public T GuidePanel(TransformedModel model, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return GuidePanel(model, position.ToPose(), width);
        }

        public T GuidePanel(TransformedModel model, double x, double y, double z, double width)
            => GuidePanel(model, x, y, z, 0, 0, 0, width);
        public T GuidePanel(TransformedModel model, double width)
            => GuidePanel(model, 0, 0, 0, width);

        public T PanelThickness(double thickness)
        {
            PanelThicknessValue = (float)thickness;
            return (T)this;
        }

        public T OpenLeft()
        {
            DirectionValue = OpenDirection.Left;
            return (T)this;
        }

        public T OpenRight()
        {
            DirectionValue = OpenDirection.Right;
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

            BifoldDoor.Panel hingedPanel = HingedPanelValue.Value;
            BifoldDoor.Panel guidePanel = GuidePanelValue.Value;

            DoorAnimationProfile openProfile = CreateAnimationProfile(OpenAnimationValue);
            DoorAnimationProfile closeProfile = CreateAnimationProfile(CloseAnimationValue);
            DoorAnimator animator = new(openProfile, closeProfile, Restitution0Value, Restitution1Value);

            BifoldDoor door = new(hingedPanel, guidePanel, PanelThicknessValue, DirectionValue)
            {
                DoorSwitch = DoorSwitchValue,
                Animator = animator,
            };
            BuiltDoor = new BifoldDoorCommand(Key, door);
            Parent.Add(BuiltDoor);
            return BuiltDoor;


            BifoldDoorCommand ReportAndCreateEmpty(string message)
            {
                ScriptError error = new(ErrorLevel.Error, message);
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


        private readonly record struct AnimationProfile(IReadOnlyCollection<CurvePoint> CurvePoints, PidGains PidGains, TimeSpan Duration);
    }
}
