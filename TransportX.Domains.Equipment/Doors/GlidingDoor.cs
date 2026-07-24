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
    public class GlidingDoor : IDoor
    {
        public static GlidingDoor Empty(string key)
        {
            DoorAnimationProfile animationProfile = new(new Curve([(0, 0), (1, 1)]), new PidController() { K = new PidGains(1, 0, 0) }, TimeSpan.FromSeconds(1));
            DoorAnimator animator = new(animationProfile, animationProfile, 0, 0);
            return new GlidingDoor(TransformedModel.Empty(), Pose.Identity, 0, 0, 0, OpenDirection.Left)
            {
                Animator = animator,
                DoorSwitch = new Signal<bool>(false),
            };
        }


        private readonly float OpenAngle;
        private readonly float SlideDistance;
        private readonly float ArmLength;
        private readonly int Direction;

        private readonly Pose PanelOrigin;
        private readonly Pose PanelOriginOffsetInverse;

        public TransformedModel Panel { get; }

        public required DoorAnimator Animator { get; init; }
        public required Signal<bool> DoorSwitch { get; init; }

        public bool IsOpen => Animator.IsOpen;

        public GlidingDoor(TransformedModel panel, Pose panelOriginOffset, float openAngle, float slideDistance, float armLength, OpenDirection direction)
        {
            OpenAngle = openAngle;
            SlideDistance = slideDistance;
            ArmLength = armLength;
            Direction = (int)direction;

            Panel = panel;
            PanelOrigin = panelOriginOffset * Panel.BasePose;
            PanelOriginOffsetInverse = Pose.Inverse(panelOriginOffset);
        }

        public void Tick(TimeSpan elapsed)
        {
            Animator.IsOpen = DoorSwitch.Value;
            Animator.Tick(elapsed);

            float angle = -Direction * float.Lerp(0, OpenAngle, Animator.OpenRate);
            (float sinAngle, float cosAngle) = float.SinCos(angle * 0.5f);
            Quaternion rotation = new(0, sinAngle, 0, cosAngle);

            float slideZ = -Direction * (SlideDistance * Animator.OpenRate); // レールスライド移動量
            float inwardX = ArmLength * (1 - float.Cos(angle)); // クランクアーム変位

            Panel.BasePose = PanelOriginOffsetInverse * new Pose(inwardX, 0, slideZ, rotation) * PanelOrigin;
        }
    }
}
