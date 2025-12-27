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

        public Matrix4x4 BaseTransform
        {
            get => field;
            set
            {
                field = value;
                Matrix4x4.Invert(value, out Matrix4x4 baseTransformInverse);
                BaseTransformInverse = baseTransformInverse;
            }
        }
        public Matrix4x4 BaseTransformInverse { get; private set; }

        public virtual Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

        protected LocatedModel(IModel model, Matrix4x4 transform, bool setTransform)
        {
            Model = model;
            BaseTransform = transform;
            if (setTransform) Transform = transform;
        }

        public LocatedModel(IModel model, Matrix4x4 transform) : this(model, transform, true)
        {
        }

        public virtual void Draw(LocatedDrawContext context)
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
