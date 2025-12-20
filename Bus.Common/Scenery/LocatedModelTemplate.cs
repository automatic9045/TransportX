using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class LocatedModelTemplate
    {
        public IModel Model { get; }
        public Matrix4x4 Transform { get; }

        public LocatedModelTemplate(IModel model, Matrix4x4 transform)
        {
            Model = model;
            Transform = transform;
        }

        public virtual LocatedModel Build()
        {
            return new LocatedModel(Model, Transform);
        }
    }
}
