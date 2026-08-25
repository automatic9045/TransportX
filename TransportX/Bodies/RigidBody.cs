using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Bodies
{
    public class RigidBody : WorldObject, IMovable, IDisposable
    {
        public BodyStructure Structure { get; }

        public Vector3 Velocity => Structure.RootModel is null ? Vector3.NaN
            : Structure.RootModel is BodyTransformedModel bodyModel ? bodyModel.Velocity : Vector3.Zero;
        public Vector3 AngularVelocity => Structure.RootModel is null ? Vector3.NaN
            : Structure.RootModel is BodyTransformedModel bodyModel ? bodyModel.AngularVelocity : Vector3.Zero;

        public RigidBody(IPhysicsHost physicsHost, WorldPose worldPose) : base(worldPose)
        {
            Structure = new BodyStructure(physicsHost);
        }

        public RigidBody(IPhysicsHost physicsHost) : this(physicsHost, WorldPose.Zero)
        {
        }

        public virtual void Dispose()
        {
            Structure.Dispose();
        }

        public virtual void SetFromCamera(ChunkIndex fromCamera)
        {
            Structure.SetFromCamera(fromCamera);
        }

        protected new ChunkIndex Locate(WorldPose worldPose) => TeleportTo(worldPose);
        protected new ChunkIndex Locate(ChunkIndex chunkIndex, Pose pose) => TeleportTo(new WorldPose(chunkIndex, pose));
        protected new ChunkIndex Move(Pose delta) => TeleportTo(delta * WorldPose);

        protected virtual ChunkIndex TeleportTo(WorldPose worldPose)
        {
            ChunkIndex oldChunk = WorldPose.Chunk;
            ChunkIndex normalizedOffset = base.Locate(worldPose);

            ChunkIndex chunkOffset = WorldPose.Chunk - oldChunk;
            if (!chunkOffset.IsZero)
            {
                foreach (TransformedModel model in Structure)
                {
                    if (model is CollidableTransformedModel collidableModel)
                    {
                        collidableModel.Shift(chunkOffset);
                    }
                }
            }

            foreach (TransformedModel model in Structure)
            {
                model.Pose = model.BasePose * WorldPose.Pose;
            }

            return normalizedOffset;
        }

        public virtual void SubTick(TimeSpan elapsed)
        {
            if (Structure.RootModel is null) return;

            WorldPose worldPose = new(WorldPose.Chunk, Structure.RootModel.BasePoseInverse * Structure.RootModel.Pose);
            ChunkIndex oldChunk = WorldPose.Chunk;

            base.Locate(worldPose);

            ChunkIndex chunkOffset = WorldPose.Chunk - oldChunk;
            if (!chunkOffset.IsZero)
            {
                foreach (TransformedModel model in Structure)
                {
                    if (model is CollidableTransformedModel collidableModel)
                    {
                        collidableModel.Shift(chunkOffset);
                    }
                }
            }

            foreach (TransformedModel model in Structure)
            {
                if (model is not DynamicTransformedModel)
                {
                    model.Pose = model.BasePose * WorldPose.Pose;
                }
            }
        }

        public virtual void Tick(TimeSpan elapsed)
        {
        }

        public virtual void Draw<TCuller>(in TransformedDrawContext context, in TCuller culler) where TCuller : struct, ICullingVolume
        {
            Structure.Draw(context, culler);
        }
    }
}
