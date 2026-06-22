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
    public class RigidBody : WorldObject, IDisposable, IDrawable
    {
        public BodyStructure Structure { get; }

        public override Vector3 Velocity => Structure.RootModel is null ? Vector3.NaN
            : Structure.RootModel is BodyTransformedModel bodyModel ? bodyModel.Velocity : Vector3.Zero;
        public Vector3 AngularVelocity => Structure.RootModel is null ? Vector3.NaN
            : Structure.RootModel is BodyTransformedModel bodyModel ? bodyModel.AngularVelocity : Vector3.Zero;

        public RigidBody(IPhysicsHost physicsHost, WorldPose worldPose) : base(worldPose)
        {
            Structure = new BodyStructure(physicsHost);

            Moved += _ =>
            {
                foreach (TransformedModel model in Structure)
                {
                    if (model is not DynamicTransformedModel) model.Pose = model.BasePose * WorldPose.Pose;
                }
            };
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

        protected virtual ChunkIndex TeleportTo(WorldPose worldPose)
        {
            ChunkIndex chunkOffset = Locate(worldPose);
            foreach (TransformedModel model in Structure)
            {
                model.Pose = model.BasePose * WorldPose.Pose;
            }

            return chunkOffset;
        }

        public virtual void SubTick(TimeSpan elapsed)
        {
            if (Structure.RootModel is null) return;

            WorldPose worldPose = new(WorldPose.Chunk, Structure.RootModel!.BasePoseInverse * Structure.RootModel.Pose);
            ChunkIndex chunkOffset = Locate(worldPose);
            if (!chunkOffset.IsZero)
            {
                foreach (TransformedModel model in Structure)
                {
                    if (model is DynamicTransformedModel dynamicModel)
                    {
                        dynamicModel.Shift(chunkOffset);
                    }
                    else
                    {
                        model.Pose = model.BasePose * WorldPose.Pose;
                    }
                }
            }
        }

        public virtual void Tick(TimeSpan elapsed)
        {
        }

        public virtual void Draw(in TransformedDrawContext context)
        {
            Structure.Draw(context);
        }
    }
}
