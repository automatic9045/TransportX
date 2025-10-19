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
        public Matrix4x4 InitialLocator { get; }
        public Matrix4x4 InitialLocatorInverse { get; }
        public virtual Matrix4x4 Locator { get; set; } = Matrix4x4.Identity;

        protected LocatedModel(IModel model, Matrix4x4 locator, bool setLocator)
        {
            Model = model;
            InitialLocator = locator;
            Matrix4x4.Invert(locator, out Matrix4x4 initialLocatorInverse);
            InitialLocatorInverse = initialLocatorInverse;
            if (setLocator) Locator = locator;
        }

        public LocatedModel(IModel model, Matrix4x4 locator) : this(model, locator, true)
        {
        }

        public void Draw(DrawContext context)
        {
            ConstantBuffer cb = new ConstantBuffer()
            {
                World = Matrix4x4.Transpose(Locator * context.PlateOffset.Transform),
                View = Matrix4x4.Transpose(context.View),
                Projection = Matrix4x4.Transpose(context.Projection),
            };
            context.DeviceContext.UpdateSubresource(cb, context.ConstantBuffer);

            Model.Draw(context.DeviceContext);
        }
    }
}
