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
        public Pose ColliderToBase => Model.Collider.Offset * Pose;

        public KinematicLocatedModelTemplate(IPhysicsHost physicsHost, ICollidableModel model, Pose pose) : base(model, pose)
        {
            PhysicsHost = physicsHost;
            Model = model;
        }

        public static LocatedModelTemplate CreateKinematicOrNonCollision(IPhysicsHost physicsHost, IModel model, Pose pose)
        {
            return model is ICollidableModel collidableModel
                ? new KinematicLocatedModelTemplate(physicsHost, collidableModel, pose) : new LocatedModelTemplate(model, pose);
        }

        public KinematicLocatedModel BuildKinematic(Converter<Pose, Pose> poseConverter)
        {
            Pose pose = poseConverter(Pose);
            return KinematicLocatedModel.Create(PhysicsHost, Model, pose);
        }

        public override LocatedModel Build(Converter<Pose, Pose> poseConverter) => BuildKinematic(poseConverter);
    }
}
