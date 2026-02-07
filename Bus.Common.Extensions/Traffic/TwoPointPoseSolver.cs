using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;

namespace Bus.Common.Extensions.Traffic
{
    public class TwoPointPoseSolver : IPoseSolver
    {
        private readonly float FrontOffset;
        private readonly float RearOffset;

        public int PlateX { get; private set; }
        public int PlateZ { get; private set; }
        public Pose Pose { get; private set; }
        public Vector3 Velocity { get; private set; }

        public event EventHandler? Moved;

        public TwoPointPoseSolver(float frontOffset, float rearOffset)
        {
            FrontOffset = frontOffset;
            RearOffset = rearOffset;
        }

        public void Tick(IReadOnlyList<LanePathView> pathViewHistory, LanePathView pathView, float viewS, TimeSpan elapsed)
        {
            Pose front = GetPoseFromHistory(viewS + FrontOffset);
            Pose rear = GetPoseFromHistory(viewS + RearOffset);

            Pose GetPoseFromHistory(float convS)
            {
                if (0 <= convS)
                {
                    return pathView.GetPose(convS);
                }

                float distanceBack = -convS;
                for (int i = 0; i < pathViewHistory.Count; i++)
                {
                    LanePathView history = pathViewHistory[i];

                    if (distanceBack <= history.Source.Length)
                    {
                        return GetWorldPose(history, history.Source.Length - distanceBack);
                    }

                    distanceBack -= history.Source.Length;
                }

                LanePathView oldest = 0 < pathViewHistory.Count ? pathViewHistory[pathViewHistory.Count - 1] : pathView;
                return GetWorldPose(oldest, convS);


                Pose GetWorldPose(in LanePathView subPathView, float convS)
                {
                    PlateOffset plateOffset = pathView.Source.Owner.GetPlateOffset(subPathView.Source.Owner);
                    return subPathView.GetPose(convS) * plateOffset.Pose;
                }
            }

            Vector3 forward = Vector3.Normalize(front.Position - rear.Position);
            Vector3 position = rear.Position - forward * RearOffset;
            Vector3 up = Vector3.Normalize(Pose.TransformNormal(Vector3.UnitY, front) + Pose.TransformNormal(Vector3.UnitY, rear));

            Pose pose = Pose.CreateWorldLH(position, forward, up);

            PlateX = pathView.Source.Owner.PlateX;
            PlateZ = pathView.Source.Owner.PlateZ;
            Pose = pose;

            Moved?.Invoke(this, EventArgs.Empty);
        }
    }
}
