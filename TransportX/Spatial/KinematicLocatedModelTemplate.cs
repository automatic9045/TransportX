using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class KinematicLocatedModelTemplate : LocatedModelTemplate
    {
        private readonly IPhysicsHost PhysicsHost;

        public new ICollidableModel Model { get; }
        public Pose ColliderToBase => Model.Collider.Offset * Pose;

        public override event EventHandler<TemplateBuiltEventArgs<LocatedModelTemplate, LocatedModel>>? Built;

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
            KinematicLocatedModel locatedModel = KinematicLocatedModel.Create(PhysicsHost, Model, pose);
            Built?.Invoke(this, new TemplateBuiltEventArgs<LocatedModelTemplate, LocatedModel>(this, locatedModel));
            return locatedModel;
        }

        public override LocatedModel Build(Converter<Pose, Pose> poseConverter) => BuildKinematic(poseConverter);
    }
}
