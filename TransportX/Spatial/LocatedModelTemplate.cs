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

        public LocatedModel BuildVisual()
        {
            LocatedModel model = new(Model, Pose);
            Built?.Invoke(this, new TemplateBuiltEventArgs<LocatedModelTemplate, LocatedModel>(this, model));
            return model;
        }

        public virtual LocatedModel Build() => BuildVisual();
    }
}
