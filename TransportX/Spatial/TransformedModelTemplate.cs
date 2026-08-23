using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class TransformedModelTemplate
    {
        public ModelResourceSet Resource { get; }
        public Pose Pose { get; }

        public virtual event EventHandler<TemplateBuiltEventArgs<TransformedModelTemplate, TransformedModel>>? Built;

        public TransformedModelTemplate(in ModelResourceSet resource, Pose pose)
        {
            Resource = resource;
            Pose = pose;
        }

        public TransformedModel BuildVisual(Converter<Pose, Pose> poseConverter)
        {
            Pose pose = poseConverter(Pose);
            TransformedModel model = new(Resource, pose);
            Built?.Invoke(this, new TemplateBuiltEventArgs<TransformedModelTemplate, TransformedModel>(this, model));
            return model;
        }

        public virtual TransformedModel Build(Converter<Pose, Pose> poseConverter) => BuildVisual(poseConverter);
    }
}
