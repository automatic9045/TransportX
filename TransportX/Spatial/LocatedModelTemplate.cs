using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class LocatedModelTemplate
    {
        public IModel Model { get; }
        public Pose Pose { get; }

        public virtual event EventHandler<TemplateBuiltEventArgs<LocatedModelTemplate, LocatedModel>>? Built;

        public LocatedModelTemplate(IModel model, Pose pose)
        {
            Model = model;
            Pose = pose;
        }

        public LocatedModel BuildVisual(Converter<Pose, Pose> poseConverter)
        {
            Pose pose = poseConverter(Pose);
            LocatedModel model = new(Model, pose);
            Built?.Invoke(this, new TemplateBuiltEventArgs<LocatedModelTemplate, LocatedModel>(this, model));
            return model;
        }

        public virtual LocatedModel Build(Converter<Pose, Pose> poseConverter) => BuildVisual(poseConverter);
        public LocatedModel Build() => Build(pose => pose);
    }
}
