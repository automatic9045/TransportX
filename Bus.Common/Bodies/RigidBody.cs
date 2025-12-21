using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;

namespace Bus.Common.Bodies
{
    public class RigidBody : LocatableObject, IDisposable, IDrawable
    {
        public BodyStructure Structure { get; }

        public Vector3 LinearVelocity => Structure.RootModel is null ? Vector3.NaN
            : Structure.RootModel is CollidableLocatedModel collidable ? collidable.LinearVelocity : Vector3.Zero;
        public Vector3 AngularVelocity => Structure.RootModel is null ? Vector3.NaN
            : Structure.RootModel is CollidableLocatedModel collidable ? collidable.AngularVelocity : Vector3.Zero;

        public RigidBody(IPhysicsHost physicsHost, int plateX, int plateZ, Matrix4x4 transform) : base(plateX, plateZ, transform)
        {
            Structure = new BodyStructure(physicsHost, () => Transform);

            Moved += (sender, e) =>
            {
                LocatedModel? rootModel = Structure.RootModel;
                if (rootModel is not null && rootModel is not DynamicLocatedModel) rootModel.Transform = Transform;
            };
        }

        public RigidBody(IPhysicsHost physicsHost, int plateX, int plateZ, SixDoF position) : this(physicsHost, plateX, plateZ, position.CreateTransform())
        {
        }

        public RigidBody(IPhysicsHost physicsHost) : this(physicsHost, 0, 0, Matrix4x4.Identity)
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

        public virtual void SubTick(TimeSpan elapsed)
        {
            if (Structure.RootModel is null) return;

            PlateOffset plateOffset = Locate(PlateX, PlateZ, Structure.RootModel!.BaseTransformInverse * Structure.RootModel.Transform);
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
                        model.Transform = model.BaseTransform * Transform;
                    }
                }
            }
        }

        public virtual void Tick(TimeSpan elapsed)
        {
        }

        public virtual void Draw(LocatedDrawContext context)
        {
            Structure.Draw(context);
        }
    }
}
