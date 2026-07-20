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
            return new BifoldDoor(TransformedModel.Empty(), TransformedModel.Empty(), 1, 1, 0.1f)
            {
                Animator = animator,
                DoorSwitch = new Signal<bool>(false),
            };
        }


        private readonly Coefficients Coeff;

        private readonly Pose HingedPanelOrigin;
        private readonly Pose GuidePanelOrigin;

        public TransformedModel HingedPanel { get; }
        public TransformedModel GuidePanel { get; }

        public required DoorAnimator Animator { get; init; }
        public required Signal<bool> DoorSwitch { get; init; }

        public bool IsOpen => Animator.IsOpen;

        public BifoldDoor(TransformedModel hingedPanel, TransformedModel guidePanel, float hingedPanelWidth, float guidePanelWidth, float doorThickness)
        {
            Coeff = new Coefficients(hingedPanelWidth, guidePanelWidth, doorThickness);

            HingedPanel = hingedPanel;
            GuidePanel = guidePanel;

            HingedPanelOrigin = HingedPanel.BasePose;
            GuidePanelOrigin = GuidePanel.BasePose;
        }

        public void Tick(TimeSpan elapsed)
        {
            Animator.IsOpen = DoorSwitch.Value;
            Animator.Tick(elapsed);

            float HingedPanelBase = float.Lerp(float.Pi / 2, Coeff.MaxOpenAngle, Animator.OpenRate);
            (float sinHingedPanelBase, float cosHingedPanelBase) = float.SinCos(HingedPanelBase);

            float sqrtPlus1 = float.Sqrt(1 + cosHingedPanelBase);
            float sqrtMinus1 = float.Sqrt(1 - cosHingedPanelBase);
            Quaternion rotation1 = new(0, -0.5f * (sqrtPlus1 - sqrtMinus1), 0, 0.5f * (sqrtPlus1 + sqrtMinus1));
            HingedPanel.BasePose = new Pose(Vector3.Zero, rotation1) * HingedPanelOrigin;

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
            Quaternion rotation2 = new(0, 0.5f * (sqrtPlus2 - sqrtMinus2), 0, 0.5f * (sqrtPlus2 + sqrtMinus2));
            GuidePanel.BasePose = new Pose(0, 0, openWidth, rotation2) * GuidePanelOrigin;


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float Square(float x) => x * x;
        }


        private readonly struct Coefficients
        {
            public float HingedPanelWidth { get; }
            public float GuidePanelWidth { get; }
            public float HalfDoorThickness { get; }

            public float HingedPanelLength { get; }
            public float GuidePanelLength { get; }

            public float MaxOpenAngle { get; }

            public float SinAlpha { get; }
            public float CosAlpha { get; }
            public float SinBeta { get; }
            public float CosBeta { get; }

            public Coefficients(float hingedPanelWidth, float guidePanelWidth, float doorThickness)
            {
                HingedPanelWidth = hingedPanelWidth;
                GuidePanelWidth = guidePanelWidth;
                HalfDoorThickness = doorThickness / 2;

                HingedPanelLength = float.Sqrt(HingedPanelWidth * HingedPanelWidth + HalfDoorThickness * HalfDoorThickness);
                GuidePanelLength = float.Sqrt(GuidePanelWidth * GuidePanelWidth + HalfDoorThickness * HalfDoorThickness);

                MaxOpenAngle = -float.Atan2(HalfDoorThickness, HingedPanelWidth) + float.Acos(GuidePanelWidth / HingedPanelLength);

                SinAlpha = HalfDoorThickness / HingedPanelLength;
                CosAlpha = HingedPanelWidth / HingedPanelLength;
                SinBeta = HalfDoorThickness / HingedPanelLength;
                CosBeta = GuidePanelWidth / GuidePanelLength;
            }
        }
    }
}
