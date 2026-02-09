using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;

namespace Bus.Common.Spatial
{
    public class LocatedModelTemplate
    {
        public IModel Model { get; }
        public Pose Pose { get; }

        public LocatedModelTemplate(IModel model, Pose pose)
        {
            Model = model;
            Pose = pose;
        }

        public virtual LocatedModel Build(Converter<Pose, Pose> poseConverter)
        {
            Pose pose = poseConverter(Pose);
            return new LocatedModel(Model, pose);
        }

        public LocatedModel Build() => Build(pose => pose);
    }
}
