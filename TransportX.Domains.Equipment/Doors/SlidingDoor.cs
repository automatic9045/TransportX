using System;
using System.Collections.Generic;
using System.Linq;
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
            return new SlidingDoor(TransformedModel.Empty(), 1)
            {
                Animator = animator,
                DoorSwitch = new Signal<bool>(false),
            };
        }


        //private const float Width = 1.005f;


        private readonly Pose PanelOrigin;
        private readonly float PanelWidth;

        public TransformedModel Panel { get; }

        public required DoorAnimator Animator { get; init; }
        public required Signal<bool> DoorSwitch { get; init; }

        public bool IsOpen => Animator.IsOpen;

        public SlidingDoor(TransformedModel panel, float panelWidth)
        {
            Panel = panel;
            PanelOrigin = Panel.BasePose;
            PanelWidth = panelWidth;
        }

        public void Tick(TimeSpan elapsed)
        {
            Animator.IsOpen = DoorSwitch.Value;
            Animator.Tick(elapsed);

            Panel.BasePose = new Pose(0, 0, Animator.OpenRate * PanelWidth) * PanelOrigin;
        }
    }
}
