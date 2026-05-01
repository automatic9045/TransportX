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
    public class KinematicTransformedModelTemplate : TransformedModelTemplate
    {
        private readonly IPhysicsHost PhysicsHost;

        private bool IsMergeProhibited = false;

        public new ICollidableModel Model { get; }
        public Pose ColliderToBase => Model.Collider.Offset * Pose;
        public bool CanMerge => !IsMergeProhibited && MergedKinematicTransformedModel.CanMerge(Model.Collider);

        public override event EventHandler<TemplateBuiltEventArgs<TransformedModelTemplate, TransformedModel>>? Built;

        public KinematicTransformedModelTemplate(IPhysicsHost physicsHost, ICollidableModel model, Pose pose) : base(model, pose)
        {
            PhysicsHost = physicsHost;
            Model = model;
        }

        public static TransformedModelTemplate CreateKinematicOrNonCollision(IPhysicsHost physicsHost, IModel model, Pose pose)
        {
            return model is ICollidableModel collidableModel
                ? new KinematicTransformedModelTemplate(physicsHost, collidableModel, pose) : new TransformedModelTemplate(model, pose);
        }

        public void ProhibitMerge()
        {
            IsMergeProhibited = true;
        }

        public KinematicTransformedModel BuildKinematic(Converter<Pose, Pose> poseConverter)
        {
            Pose pose = poseConverter(Pose);
            KinematicTransformedModel transformedModel = KinematicTransformedModel.Create(PhysicsHost, Model, pose);
            Built?.Invoke(this, new TemplateBuiltEventArgs<TransformedModelTemplate, TransformedModel>(this, transformedModel));
            return transformedModel;
        }

        public override TransformedModel Build(Converter<Pose, Pose> poseConverter) => BuildKinematic(poseConverter);
    }
}
