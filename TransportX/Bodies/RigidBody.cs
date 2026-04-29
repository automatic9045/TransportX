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
    public class RigidBody : LocatableObject, IDisposable, IDrawable
    {
        public BodyStructure Structure { get; }

        public override Vector3 Velocity => Structure.RootModel is null ? Vector3.NaN
            : Structure.RootModel is CollidableLocatedModel collidable ? collidable.Velocity : Vector3.Zero;
        public Vector3 AngularVelocity => Structure.RootModel is null ? Vector3.NaN
            : Structure.RootModel is CollidableLocatedModel collidable ? collidable.AngularVelocity : Vector3.Zero;

        public RigidBody(IPhysicsHost physicsHost, WorldPose worldPose) : base(worldPose)
        {
            Structure = new BodyStructure(physicsHost);

            Moved += _ =>
            {
                foreach (LocatedModel model in Structure)
                {
                    if (model is not DynamicLocatedModel) model.Pose = model.BasePose * WorldPose.Pose;
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

        public virtual void SetFromCamera(ChunkOffset fromCamera)
        {
            Structure.SetFromCamera(fromCamera);
        }

        protected virtual ChunkOffset TeleportTo(WorldPose worldPose)
        {
            ChunkOffset chunkOffset = Locate(worldPose);
            foreach (LocatedModel model in Structure)
            {
                model.Pose = model.BasePose * WorldPose.Pose;
            }

            return chunkOffset;
        }

        public virtual void SubTick(TimeSpan elapsed)
        {
            if (Structure.RootModel is null) return;

            WorldPose worldPose = new(WorldPose.ChunkX, WorldPose.ChunkZ, Structure.RootModel!.BasePoseInverse * Structure.RootModel.Pose);
            ChunkOffset chunkOffset = Locate(worldPose);
            if (!chunkOffset.IsZero)
            {
                foreach (LocatedModel model in Structure)
                {
                    if (model is DynamicLocatedModel dynamicModel)
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

        public virtual void Draw(in LocatedDrawContext context)
        {
            Structure.Draw(context);
        }
    }
}
