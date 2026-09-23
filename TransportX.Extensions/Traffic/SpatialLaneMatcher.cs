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
    public class SpatialLaneMatcher : IDisposable
    {
        private const float LateralTolerance = 1;
        private const float LongitudinalTolerance = 0.5f;
        private const float HysteresisMarginSquared = 1.5f;
        private const float DirectionThreshold = 0.4f;
        private const int InitialCandidateCapacity = 128;

        private readonly ITrafficEntity Entity;
        private readonly HashSet<ILanePath> CandidatePathBuffer = new(InitialCandidateCapacity);

        private bool IsDisposed = false;

        public ILanePath? PrimaryPath { get; private set; } = null;
        public EntityDirection Heading { get; private set; } = EntityDirection.Forward;
        public float S { get; private set; } = 0;
        public float SVelocity { get; private set; } = 0;

        public SpatialLaneMatcher(ITrafficEntity entity)
        {
            Entity = entity;
        }

        public void Update(ChunkCollection chunks)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            WorldPose entityPose = Entity.WorldPose;
            Vector3 entityForward = entityPose.Pose.Direction;
            float halfWidth = float.Max(0.5f, Entity.Width * 0.5f);

            CollectCandidatePaths(entityPose.Chunk, chunks, CandidatePathBuffer);

            ILanePath? bestPath = null;
            PathMatchResult bestResult = PathMatchResult.Empty;

            foreach (ILanePath path in CandidatePathBuffer)
            {
                if (TryEvaluatePath(path, entityPose, entityForward, halfWidth, out PathMatchResult result))
                {
                    float scoreSquared = result.DistanceSquared;
                    if (path == PrimaryPath) scoreSquared -= HysteresisMarginSquared;

                    if (scoreSquared < bestResult.DistanceSquared)
                    {
                        bestPath = path;
                        bestResult = new PathMatchResult(result.S, result.Heading, scoreSquared);
                    }
                }
            }

            if (bestPath != PrimaryPath)
            {
                PrimaryPath?.Exit(Entity);
                bestPath?.Enter(Entity);
                PrimaryPath = bestPath;
            }

            if (PrimaryPath is not null)
            {
                S = bestResult.S;
                Heading = bestResult.Heading;

                WorldPose pCurrent = PrimaryPath.GetWorldPose(S);
                float ds = S + 0.5f < PrimaryPath.Length ? 0.5f : -0.5f;
                WorldPose pAdjacent = PrimaryPath.GetWorldPose(float.Clamp(S + ds, 0, PrimaryPath.Length));
                Vector3 delta = pCurrent.GetOffset(pAdjacent);

                Vector3 pathPositiveTangent = 1e-6f < delta.LengthSquared()
                    ? Vector3.Normalize(delta) * float.Sign(ds)
                    : pCurrent.Pose.Direction;

                SVelocity = Vector3.Dot(Entity.Velocity, pathPositiveTangent);
            }
            else
            {
                S = 0;
                SVelocity = 0;
                Heading = EntityDirection.Forward;
            }
        }

        private void CollectCandidatePaths(ChunkIndex originChunk, ChunkCollection chunks, HashSet<ILanePath> destination)
        {
            destination.Clear();

            if (PrimaryPath is not null)
            {
                destination.Add(PrimaryPath);
                AddConnectedPaths(PrimaryPath.From, destination);
                AddConnectedPaths(PrimaryPath.To, destination);
            }

            for (int dx = -1; dx < 2; dx++)
            {
                for (int dz = -1; dz < 2; dz++)
                {
                    ChunkIndex neighborIndex = new(originChunk.X + dx, originChunk.Z + dz);
                    if (chunks.TryGetValue(neighborIndex, out Chunk? chunk))
                    {
                        for (int i = 0; i < chunk.Network.Count; i++)
                        {
                            IReadOnlyList<ILanePath> paths = chunk.Network[i].Paths;
                            for (int j = 0; j < paths.Count; j++)
                            {
                                destination.Add(paths[j]);
                            }
                        }
                    }
                }
            }

            static void AddConnectedPaths(LanePin pin, HashSet<ILanePath> dest)
            {
                LanePin? connected = pin.ConnectedPin;
                if (connected is null) return;

                for (int i = 0; i < connected.SourcePaths.Count; i++) dest.Add(connected.SourcePaths[i]);
                for (int i = 0; i < connected.DestPaths.Count; i++) dest.Add(connected.DestPaths[i]);
            }
        }

        private static bool TryEvaluatePath(ILanePath path, in WorldPose entityPose, Vector3 entityForward, float halfWidth, out PathMatchResult result)
        {
            result = PathMatchResult.Empty;
            if (path.Length < 1e-3f) return false;

            float bestS = path.ProjectToS(entityPose);

            WorldPose pathWorldPose = path.GetWorldPose(bestS);
            Vector3 pathDirection = pathWorldPose.Pose.Direction;
            float dotDirection = Vector3.Dot(entityForward, pathDirection);

            if (float.Abs(dotDirection) < DirectionThreshold) return false;

            EntityDirection heading = 0 < dotDirection ? EntityDirection.Forward : EntityDirection.Backward;
            FlowDirections requiredFlow = heading == EntityDirection.Forward ? FlowDirections.Out : FlowDirections.In;
            if (!path.Directions.HasFlag(requiredFlow)) return false;

            Pose pathPoseInv = Pose.Inverse(pathWorldPose.Pose);
            Vector3 chunkPositionDelta = (entityPose.Chunk - pathWorldPose.Chunk).Position;
            Vector3 localOffset = Pose.Transform(entityPose.Pose.Position + chunkPositionDelta, pathPoseInv);

            if (bestS < 0.05f && localOffset.Z < -LongitudinalTolerance) return false;
            if (path.Length - 0.05f < bestS && LongitudinalTolerance < localOffset.Z) return false;

            LaneWidth laneWidth = path.GetWidth(bestS);
            float maxRightLimit = laneWidth.Right + halfWidth + LateralTolerance;
            float maxLeftLimit = laneWidth.Left + halfWidth + LateralTolerance;

            if (localOffset.X < -maxLeftLimit || maxRightLimit < localOffset.X) return false;

            float distanceSquared = entityPose.GetOffset(pathWorldPose).LengthSquared();
            result = new PathMatchResult(bestS, heading, distanceSquared);
            return true;
        }

        public void Dispose()
        {
            if (IsDisposed) throw new InvalidOperationException();
            IsDisposed = true;

            PrimaryPath?.Exit(Entity);
            PrimaryPath = null;
        }

        private readonly record struct PathMatchResult(float S, EntityDirection Heading, float DistanceSquared)
        {
            public static PathMatchResult Empty => new(0, EntityDirection.Forward, float.MaxValue);
        }
    }
}
