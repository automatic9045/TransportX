using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;

namespace Bus.Common
{
    public class RigidBody : LocatableObject, IDisposable, IDrawable
    {
        private readonly LocatableObject Camera;

        public LocatedModelCollection Models { get; }

        public Vector3 LinearVelocity => Models.RootModel is null ? Vector3.NaN : Models.RootModel.LinearVelocity;
        public Vector3 AngularVelocity => Models.RootModel is null ? Vector3.NaN : Models.RootModel.AngularVelocity;

        public RigidBody(IPhysicsHost physicsHost, LocatableObject camera, int plateX, int plateZ, Matrix4x4 transform) : base(plateX, plateZ, transform)
        {
            Camera = camera;
            Models = new LocatedModelCollection(physicsHost, () => Transform);
        }

        public RigidBody(IPhysicsHost physicsHost, LocatableObject camera, int plateX, int plateZ, SixDoF position)
            : this(physicsHost, camera, plateX, plateZ, position.CreateTransform())
        {
        }

        public RigidBody(IPhysicsHost physicsHost, LocatableObject camera) : this(physicsHost, camera, 0, 0, Matrix4x4.Identity)
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual void SubTick(TimeSpan elapsed)
        {
            if (Models.RootModel is null) return;

            PlateOffset fromCamera = Camera.GetPlateOffset(this);
            foreach (LocatedModel model in Models)
            {
                if (model is CollidableLocatedModel dynamicModel) dynamicModel.Update(fromCamera);
            }

            PlateOffset plateOffset = Locate(PlateX, PlateZ, Models.RootModel!.BaseTransformInverse * Models.RootModel.Transform);
            if (!plateOffset.IsZero)
            {
                foreach (LocatedModel model in Models)
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

        public virtual void Draw(DrawContext context)
        {
            Models.Draw(context);
        }
    }
}
