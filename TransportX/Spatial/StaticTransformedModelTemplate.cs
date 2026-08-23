using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class StaticTransformedModelTemplate : TransformedModelTemplate
    {
        private readonly IPhysicsHost PhysicsHost;

        private bool IsMergeProhibited = false;

        public ICollider Collider => Resource.GetCollider();
        public Pose ColliderToBase => Collider.Offset * Pose;
        public bool CanMerge => !IsMergeProhibited && MergedStaticTransformedModel.CanMerge(Collider);

        public override event EventHandler<TemplateBuiltEventArgs<TransformedModelTemplate, TransformedModel>>? Built;

        public StaticTransformedModelTemplate(IPhysicsHost physicsHost, in ModelResourceSet resource, Pose pose) : base(resource, pose)
        {
            PhysicsHost = physicsHost;
        }

        public static TransformedModelTemplate CreateStaticOrNonCollision(IPhysicsHost physicsHost, in ModelResourceSet resource, Pose pose)
        {
            return resource.Collider is null
                ? new TransformedModelTemplate(resource, pose) : new StaticTransformedModelTemplate(physicsHost, resource, pose);
        }

        public void ProhibitMerge()
        {
            IsMergeProhibited = true;
        }

        public StaticTransformedModel BuildStatic(Converter<Pose, Pose> poseConverter)
        {
            Pose pose = poseConverter(Pose);
            StaticTransformedModel transformedModel = StaticTransformedModel.Create(PhysicsHost, Resource, pose);
            Built?.Invoke(this, new TemplateBuiltEventArgs<TransformedModelTemplate, TransformedModel>(this, transformedModel));
            return transformedModel;
        }

        public override TransformedModel Build(Converter<Pose, Pose> poseConverter) => BuildStatic(poseConverter);
    }
}
