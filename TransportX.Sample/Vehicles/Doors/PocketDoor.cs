using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

using TransportX.Sample.Mathematics;
using TransportX.Sample.Vehicles.Interfaces;

namespace TransportX.Sample.Vehicles.Doors
{
    internal class PocketDoor
    {
        private const float Width = 1.005f;


        private readonly DoorSwitch DoorSwitch;
        private readonly LocatedModel Model;
        private readonly Pose Origin;

        private readonly DoorAnimator Animator;

        public bool IsOpen => Animator.IsOpen;

        public PocketDoor(DoorSwitch doorSwitch, LocatedModel model)
        {
            DoorSwitch = doorSwitch;
            Model = model;
            Origin = Model.BasePose;

            Diagram openDiagram = new([
                new DiagramPoint(0, 0),
                new DiagramPoint(0.7f, 0.9f),
                new DiagramPoint(0.9f, 0.94f),
                new DiagramPoint(1, 1),
            ]);
            PIDController openPID = new() { K = (20, 0, 5), };
            DoorAnimationProfile openProfile = new(openDiagram, openPID, TimeSpan.FromSeconds(2.5f));

            Diagram closeDiagram = new([
                new DiagramPoint(0, 0),
                new DiagramPoint(0.1f, 0.06f),
                new DiagramPoint(0.3f, 0.1f),
                new DiagramPoint(1, 1),
            ]);
            PIDController closePID = new() { K = (20, 0, 5), };
            DoorAnimationProfile closeProfile = new(closeDiagram, closePID, TimeSpan.FromSeconds(2.5f));

            Animator = new DoorAnimator(openProfile, closeProfile, 0.01f, 0.01f);
        }

        public void Tick(TimeSpan elapsed)
        {
            Animator.IsOpen = DoorSwitch.IsRearOpen;
            Animator.Tick(elapsed);
            
            Model.BasePose = new Pose(0, 0, Animator.OpenRate * Width) * Origin;
        }
    }
}
