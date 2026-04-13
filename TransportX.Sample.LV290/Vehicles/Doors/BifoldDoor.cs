using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

using TransportX.Sample.LV290.Mathematics;
using TransportX.Sample.LV290.Vehicles.Interfaces;

namespace TransportX.Sample.LV290.Vehicles.Doors
{
    internal class BifoldDoor
    {
        private const float Door1Width = 0.511f;
        private const float Door2Width = 0.51f;
        private const float HalfDoorThickness = 0.01f;

        private static readonly float Door1Length = float.Sqrt(Door1Width * Door1Width + HalfDoorThickness * HalfDoorThickness);
        private static readonly float Door2Length = float.Sqrt(Door2Width * Door2Width + HalfDoorThickness * HalfDoorThickness);

        private static readonly float MaxOpenAngle;

        private static readonly float SinAlpha;
        private static readonly float CosAlpha;
        private static readonly float SinBeta;
        private static readonly float CosBeta;

        static BifoldDoor()
        {
            SinAlpha = HalfDoorThickness / Door1Length;
            CosAlpha = Door1Width / Door1Length;
            SinBeta = HalfDoorThickness / Door1Length;
            CosBeta = Door2Width / Door2Length;

            MaxOpenAngle = -float.Atan2(HalfDoorThickness, Door1Width) + float.Acos(Door2Width / Door1Length);
        }


        private readonly DoorSwitch DoorSwitch;

        private readonly LocatedModel Door1Model;
        private readonly LocatedModel Door2Model;

        private readonly Pose Door1Origin;
        private readonly Pose Door2Origin;

        private readonly DoorAnimator Animator;

        public bool IsOpen => Animator.IsOpen;

        public BifoldDoor(DoorSwitch doorSwitch, LocatedModel door1Model, LocatedModel door2Model)
        {
            DoorSwitch = doorSwitch;

            Door1Model = door1Model;
            Door2Model = door2Model;

            Door1Origin = Door1Model.BasePose;
            Door2Origin = Door2Model.BasePose;

            Diagram openDiagram = new([
                new DiagramPoint(0, 0),
                new DiagramPoint(0.2f, 0.05f),
                new DiagramPoint(0.65f, 0.8f),
                new DiagramPoint(1, 1),
            ]);
            PIDController openPID = new() { K = (10, 0, 2), };
            DoorAnimationProfile openProfile = new(openDiagram, openPID, TimeSpan.FromSeconds(3.5f));

            Diagram closeDiagram = new([
                new DiagramPoint(0, 0),
                new DiagramPoint(0.2f, 0.1f),
                new DiagramPoint(0.4f, 0.25f),
                new DiagramPoint(1, 1),
            ]);
            PIDController closePID = new() { K = (15, 0, 1), };
            DoorAnimationProfile closeProfile = new(closeDiagram, closePID, TimeSpan.FromSeconds(3));

            Animator = new DoorAnimator(openProfile, closeProfile, 0.01f, 0.1f);
        }

        public void Tick(TimeSpan elapsed)
        {
            Animator.IsOpen = DoorSwitch.IsFrontOpen;
            Animator.Tick(elapsed);

            float door1Base = float.Lerp(float.Pi / 2, MaxOpenAngle, Animator.OpenRate);
            (float sinDoor1Base, float cosDoor1Base) = float.SinCos(door1Base);

            float sqrtPlus1 = float.Sqrt(1 + cosDoor1Base);
            float sqrtMinus1 = float.Sqrt(1 - cosDoor1Base);
            Quaternion rotation1 = new(0, -0.5f * (sqrtPlus1 - sqrtMinus1), 0, 0.5f * (sqrtPlus1 + sqrtMinus1));
            Door1Model.BasePose = new Pose(Vector3.Zero, rotation1) * Door1Origin;

            float cosDoor1 = cosDoor1Base * CosAlpha - sinDoor1Base * SinAlpha;
            float sinDoor1 = sinDoor1Base * CosAlpha + cosDoor1Base * SinAlpha;

            float cosDoor2 = float.Clamp(Door1Length / Door2Length * cosDoor1, -1, 1);

            float sinDoor2Square = 1 - cosDoor2 * cosDoor2;
            float sinDoor2 = float.Sqrt(0 < sinDoor2Square ? sinDoor2Square : 0);

            float cosDoor2Base = float.Clamp(CosBeta * cosDoor2 + SinBeta * sinDoor2, -1, 1);

            float cosDoor1Door2 = cosDoor1 * cosDoor2 - sinDoor1 * sinDoor2;

            float widthSquare = Door1Length * Door1Length + Door2Length * Door2Length - 2 * Door1Length * Door2Length * cosDoor1Door2;
            float width = float.Sqrt(0 < widthSquare ? widthSquare : 0);
            float openWidth = Door1Width + Door2Width - width;

            float sqrtPlus2 = float.Sqrt(1 + cosDoor2Base);
            float sqrtMinus2 = float.Sqrt(1 - cosDoor2Base);
            Quaternion rotation2 = new(0, 0.5f * (sqrtPlus2 - sqrtMinus2), 0, 0.5f * (sqrtPlus2 + sqrtMinus2));
            Door2Model.BasePose = new Pose(0, 0, openWidth, rotation2) * Door2Origin;
        }
    }
}
