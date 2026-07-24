using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;
using TransportX.Mathematics;
using TransportX.Spatial;

namespace TransportX.Domains.Equipment.Doors
{
    public class BifoldDoor : IDoor
    {
        public static BifoldDoor Empty(string key)
        {
            DoorAnimationProfile animationProfile = new(new Curve([(0, 0), (1, 1)]), new PidController() { K = new PidGains(1, 0, 0) }, TimeSpan.FromSeconds(1));
            DoorAnimator animator = new(animationProfile, animationProfile, 0, 0);
            return new BifoldDoor(CreatePanel(), CreatePanel(), 0.1f, OpenDirection.Left)
            {
                Animator = animator,
                DoorSwitch = new Signal<bool>(false),
            };


            static Panel CreatePanel() => new(TransformedModel.Empty(), Pose.Identity, 1);
        }


        private readonly Coefficients Coeff;
        private readonly int SlideSign;

        private readonly Pose HingedPanelOrigin;
        private readonly Pose GuidePanelOrigin;
        private readonly Pose HingedPanelOriginOffsetInverse;
        private readonly Pose GuidePanelOriginOffsetInverse;

        public TransformedModel HingedPanel { get; }
        public TransformedModel GuidePanel { get; }

        public required DoorAnimator Animator { get; init; }
        public required Signal<bool> DoorSwitch { get; init; }

        public bool IsOpen => Animator.IsOpen;

        public BifoldDoor(Panel hingedPanel, Panel guidePanel, float panelThickness, OpenDirection direction)
        {
            Coeff = new Coefficients(hingedPanel.Width, guidePanel.Width, panelThickness, direction);

            HingedPanel = hingedPanel.Model;
            GuidePanel = guidePanel.Model;

            HingedPanelOrigin = hingedPanel.OriginOffset * HingedPanel.BasePose;
            GuidePanelOrigin = guidePanel.OriginOffset * GuidePanel.BasePose;
            HingedPanelOriginOffsetInverse = Pose.Inverse(hingedPanel.OriginOffset);
            GuidePanelOriginOffsetInverse = Pose.Inverse(guidePanel.OriginOffset);

            SlideSign = GuidePanelOrigin.Position.Z - HingedPanelOrigin.Position.Z < 0 ? 1 : -1;
        }

        public void Tick(TimeSpan elapsed)
        {
            Animator.IsOpen = DoorSwitch.Value;
            Animator.Tick(elapsed);

            float HingedPanelBase = float.Lerp(float.Pi / 2, Coeff.MaxOpenAngle, Animator.OpenRate);
            (float sinHingedPanelBase, float cosHingedPanelBase) = float.SinCos(HingedPanelBase);

            float sqrtPlus1 = float.Sqrt(1 + cosHingedPanelBase);
            float sqrtMinus1 = float.Sqrt(1 - cosHingedPanelBase);
            Quaternion rotation1 = new(0, Coeff.Direction * 0.5f * (sqrtPlus1 - sqrtMinus1), 0, 0.5f * (sqrtPlus1 + sqrtMinus1));
            HingedPanel.BasePose = HingedPanelOriginOffsetInverse * new Pose(Vector3.Zero, rotation1) * HingedPanelOrigin;

            float cosHingedPanel = cosHingedPanelBase * Coeff.CosAlpha - sinHingedPanelBase * Coeff.SinAlpha;
            float sinHingedPanel = sinHingedPanelBase * Coeff.CosAlpha + cosHingedPanelBase * Coeff.SinAlpha;

            float cosGuidePanel = float.Clamp(Coeff.HingedPanelLength / Coeff.GuidePanelLength * cosHingedPanel, -1, 1);

            float sinGuidePanelSquare = 1 - Square(cosGuidePanel);
            float sinGuidePanel = float.Sqrt(0 < sinGuidePanelSquare ? sinGuidePanelSquare : 0);

            float cosGuidePanelBase = float.Clamp(Coeff.CosBeta * cosGuidePanel + Coeff.SinBeta * sinGuidePanel, -1, 1);

            float cosHingedPanelGuidePanel = cosHingedPanel * cosGuidePanel - sinHingedPanel * sinGuidePanel;

            float widthSquare = Square(Coeff.HingedPanelLength) + Square(Coeff.GuidePanelLength)
                - 2 * Coeff.HingedPanelLength * Coeff.GuidePanelLength * cosHingedPanelGuidePanel;
            float width = float.Sqrt(0 < widthSquare ? widthSquare : 0);
            float openWidth = Coeff.HingedPanelWidth + Coeff.GuidePanelWidth - width;

            float sqrtPlus2 = float.Sqrt(1 + cosGuidePanelBase);
            float sqrtMinus2 = float.Sqrt(1 - cosGuidePanelBase);
            Quaternion rotation2 = new(0, -Coeff.Direction * 0.5f * (sqrtPlus2 - sqrtMinus2), 0, 0.5f * (sqrtPlus2 + sqrtMinus2));
            GuidePanel.BasePose = GuidePanelOriginOffsetInverse * new Pose(0, 0, SlideSign * openWidth, rotation2) * GuidePanelOrigin;


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float Square(float x) => x * x;
        }


        private readonly struct Coefficients
        {
            public float HingedPanelWidth { get; }
            public float GuidePanelWidth { get; }
            public float HalfPanelThickness { get; }
            public int Direction { get; }

            public float HingedPanelLength { get; }
            public float GuidePanelLength { get; }

            public float MaxOpenAngle { get; }

            public float SinAlpha { get; }
            public float CosAlpha { get; }
            public float SinBeta { get; }
            public float CosBeta { get; }

            public Coefficients(float hingedPanelWidth, float guidePanelWidth, float panelThickness, OpenDirection direction)
            {
                HingedPanelWidth = hingedPanelWidth;
                GuidePanelWidth = guidePanelWidth;
                HalfPanelThickness = panelThickness / 2;
                Direction = (int)direction;

                HingedPanelLength = float.Sqrt(HingedPanelWidth * HingedPanelWidth + HalfPanelThickness * HalfPanelThickness);
                GuidePanelLength = float.Sqrt(GuidePanelWidth * GuidePanelWidth + HalfPanelThickness * HalfPanelThickness);

                MaxOpenAngle = -float.Atan2(HalfPanelThickness, HingedPanelWidth) + float.Acos(float.Clamp(GuidePanelWidth / HingedPanelLength, -1, 1));

                SinAlpha = HalfPanelThickness / HingedPanelLength;
                CosAlpha = HingedPanelWidth / HingedPanelLength;
                SinBeta = HalfPanelThickness / HingedPanelLength;
                CosBeta = GuidePanelWidth / GuidePanelLength;
            }
        }


        public readonly record struct Panel(TransformedModel Model, Pose OriginOffset, float Width);
    }
}
