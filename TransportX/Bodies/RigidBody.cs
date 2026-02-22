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

        public RigidBody(IPhysicsHost physicsHost, int plateX, int plateZ, Pose pose) : base(plateX, plateZ, pose)
        {
            Structure = new BodyStructure(physicsHost);

            Moved += (sender, e) =>
            {
                LocatedModel? rootModel = Structure.RootModel;
                if (rootModel is not null && rootModel is not DynamicLocatedModel) rootModel.Pose = Pose;
            };
        }

        public RigidBody(IPhysicsHost physicsHost, int plateX, int plateZ, SixDoF position) : this(physicsHost, plateX, plateZ, position.ToPose())
        {
        }

        public RigidBody(IPhysicsHost physicsHost) : this(physicsHost, 0, 0, Pose.Identity)
        {
        }

        public virtual void Dispose()
        {
            Structure.Dispose();
        }

        public virtual void SetFromCamera(PlateOffset fromCamera)
        {
            Structure.SetFromCamera(fromCamera);
        }

        protected virtual PlateOffset TeleportTo(int plateX, int plateZ, Pose pose)
        {
            PlateOffset plateOffset = Locate(plateX, plateZ, pose);
            foreach (LocatedModel model in Structure)
            {
                model.Pose = model.BasePose * Pose;
            }

            return plateOffset;
        }

        public virtual void SubTick(TimeSpan elapsed)
        {
            if (Structure.RootModel is null) return;

            PlateOffset plateOffset = Locate(PlateX, PlateZ, Structure.RootModel!.BasePoseInverse * Structure.RootModel.Pose);
            if (!plateOffset.IsZero)
            {
                foreach (LocatedModel model in Structure)
                {
                    if (model is DynamicLocatedModel dynamicModel)
                    {
                        dynamicModel.Shift(plateOffset);
                    }
                    else
                    {
                        model.Pose = model.BasePose * Pose;
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
