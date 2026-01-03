using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class KinematicLocatedModelTemplate : LocatedModelTemplate
    {
        private readonly IPhysicsHost PhysicsHost;

        public new ICollidableModel Model { get; }
        public Matrix4x4 ColliderToBase => Model.Collider.Offset * Transform;

        public KinematicLocatedModelTemplate(IPhysicsHost physicsHost, ICollidableModel model, Matrix4x4 transform) : base(model, transform)
        {
            PhysicsHost = physicsHost;
            Model = model;
        }

        public static LocatedModelTemplate CreateKinematicOrNonCollision(IPhysicsHost physicsHost, IModel model, Matrix4x4 transform)
        {
            return model is ICollidableModel collidableModel
                ? new KinematicLocatedModelTemplate(physicsHost, collidableModel, transform) : new LocatedModelTemplate(model, transform);
        }

        public KinematicLocatedModel BuildKinematic(Converter<Matrix4x4, Matrix4x4> transformConverter)
        {
            Matrix4x4 transform = transformConverter(Transform);
            return KinematicLocatedModel.Create(PhysicsHost, Model, transform);
        }

        public override LocatedModel Build(Converter<Matrix4x4, Matrix4x4> transformConverter) => BuildKinematic(transformConverter);
    }
}
