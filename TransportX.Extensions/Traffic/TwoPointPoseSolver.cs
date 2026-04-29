using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Spatial;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public class TwoPointPoseSolver : LocatableObject, IPoseSolver
    {
        private readonly float FrontOffset;
        private readonly float RearOffset;

        public TwoPointPoseSolver(float frontOffset, float rearOffset)
        {
            FrontOffset = frontOffset;
            RearOffset = rearOffset;
        }

        public void Tick(IReadOnlyList<LanePathView> pathViewHistory, LanePathView pathView, float viewS, TimeSpan elapsed)
        {
            WorldPose front = GetWorldPoseFromHistory(viewS + FrontOffset);
            WorldPose rear = GetWorldPoseFromHistory(viewS + RearOffset);


            WorldPose GetWorldPoseFromHistory(float convS)
            {
                if (0 <= convS)
                {
                    return pathView.GetWorldPose(convS);
                }

                float distanceBack = -convS;
                for (int i = 0; i < pathViewHistory.Count; i++)
                {
                    LanePathView history = pathViewHistory[i];

                    if (distanceBack <= history.Source.Length)
                    {
                        return history.GetWorldPose(history.Source.Length - distanceBack);
                    }

                    distanceBack -= history.Source.Length;
                }

                LanePathView oldest = 0 < pathViewHistory.Count ? pathViewHistory[pathViewHistory.Count - 1] : pathView;
                return oldest.GetWorldPose(convS);
            }

            Vector3 diff = rear.GetOffset(front);
            Vector3 forward = diff.LengthSquared() < 1e-6f ? Pose.TransformNormal(Vector3.UnitZ, rear.Pose) : Vector3.Normalize(diff);
            Vector3 position = rear.Pose.Position - forward * RearOffset;
            Vector3 up = Vector3.Normalize(Pose.TransformNormal(Vector3.UnitY, front.Pose) + Pose.TransformNormal(Vector3.UnitY, rear.Pose));

            Pose pose = Pose.CreateWorldLH(position, forward, up);
            WorldPose worldPose = rear.ChangePose(pose);
            Locate(worldPose);
        }
    }
}
