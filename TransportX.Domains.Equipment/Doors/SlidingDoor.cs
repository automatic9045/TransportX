using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;
using TransportX.Mathematics;
using TransportX.Spatial;

namespace TransportX.Domains.Equipment.Doors
{
    public class SlidingDoor : IDoor
    {
        public static SlidingDoor Empty(string key)
        {
            DoorAnimationProfile animationProfile = new(new Curve([(0, 0), (1, 1)]), new PidController() { K = new PidGains(1, 0, 0) }, TimeSpan.FromSeconds(1));
            DoorAnimator animator = new(animationProfile, animationProfile, 0, 0);
            return new SlidingDoor(TransformedModel.Empty(), Quaternion.Identity, 1)
            {
                Animator = animator,
                DoorSwitch = new Signal<bool>(false),
            };
        }


        private readonly Pose PanelOrigin;
        private readonly Quaternion PanelOriginOffsetInverse;
        private readonly float PanelWidth;

        public TransformedModel Panel { get; }

        public required DoorAnimator Animator { get; init; }
        public required Signal<bool> DoorSwitch { get; init; }

        public bool IsOpen => Animator.IsOpen;

        public SlidingDoor(TransformedModel panel, Quaternion panelOriginOffset, float panelWidth)
        {
            Panel = panel;
            PanelOrigin = panelOriginOffset * Panel.BasePose;
            PanelOriginOffsetInverse = Quaternion.Inverse(panelOriginOffset);
            PanelWidth = panelWidth;
        }

        public void Tick(TimeSpan elapsed)
        {
            Animator.IsOpen = DoorSwitch.Value;
            Animator.Tick(elapsed);

            Panel.BasePose = PanelOriginOffsetInverse * new Pose(0, 0, -Animator.OpenRate * PanelWidth) * PanelOrigin;
        }
    }
}
