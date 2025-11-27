using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class LocatedModel : IDrawable
    {
        public IModel Model { get; }
        public Matrix4x4 BaseTransform { get; }
        public Matrix4x4 BaseTransformInverse { get; }
        public virtual Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

        protected LocatedModel(IModel model, Matrix4x4 transform, bool setTransform)
        {
            Model = model;
            BaseTransform = transform;
            Matrix4x4.Invert(transform, out Matrix4x4 baseTransformInverse);
            BaseTransformInverse = baseTransformInverse;
            if (setTransform) Transform = transform;
        }

        public LocatedModel(IModel model, Matrix4x4 transform) : this(model, transform, true)
        {
        }

        public void Draw(LocatedDrawContext context)
        {
            VertexConstantBuffer vertexBuffer = new()
            {
                World = Matrix4x4.Transpose(Transform * context.PlateOffset.Transform),
                View = Matrix4x4.Transpose(context.View),
                Projection = Matrix4x4.Transpose(context.Projection),
                Light = context.Light.AsVector4(),
            };
            context.DeviceContext.UpdateSubresource(vertexBuffer, context.VertexConstantBuffer);

            Model.Draw(new(context.DeviceContext, context.VertexConstantBuffer, context.PixelConstantBuffer));
        }
    }
}
