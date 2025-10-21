using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class StaticLocatedModel : CollidableLocatedModel
    {
        public StaticHandle Handle { get; }
        public StaticReference Static => Simulation.Statics[Handle];

        protected override Matrix4x4 ColliderLocator
        {
            get => Model.Collider.OffsetInverse * Static.Pose.ToMatrix4x4() * FromCamera.TransformInverse;
            set
            {
                Static.GetDescription(out StaticDescription desc);
                desc.Pose = (Model.Collider.Offset * value * FromCamera.Transform).ToRigidPose();
                Static.ApplyDescription(desc);
            }
        }

        internal protected StaticLocatedModel(Simulation simulation, ICollidableModel model, StaticHandle handle, Matrix4x4 locator)
            : base(simulation, model, locator)
        {
            Handle = handle;
        }

        public static StaticLocatedModel Create(Simulation simulation, ICollidableModel model, Matrix4x4 locator)
        {
            StaticDescription desc = new StaticDescription((locator * model.Collider.Offset).ToRigidPose(), model.Collider.ShapeIndex);
            StaticHandle handle = simulation.Statics.Add(desc);
            return new StaticLocatedModel(simulation, model, handle, locator);
        }

        public static LocatedModel CreateStaticOrNonCollision(Simulation simulation, IModel model, Matrix4x4 locator)
        {
            return model is ICollidableModel collidableModel
                ? Create(simulation, collidableModel, locator) : new LocatedModel(model, locator);
        }
    }
}
