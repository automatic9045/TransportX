using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class LocatedModel
    {
        public IModel Model { get; }
        public Matrix4x4 InitialLocator { get; }
        public Matrix4x4 Locator { get; set; }

        public LocatedModel(IModel model, Matrix4x4 locator)
        {
            Model = model;
            InitialLocator = locator;
            Locator = locator;
        }

        public void Draw(ID3D11DeviceContext context, ID3D11Buffer constantBuffer, Matrix4x4 view, Matrix4x4 projection)
        {
            ConstantBuffer cb = new ConstantBuffer()
            {
                World = Matrix4x4.Transpose(Locator),
                View = Matrix4x4.Transpose(view),
                Projection = Matrix4x4.Transpose(projection),
            };
            context.UpdateSubresource(cb, constantBuffer);

            Model.Draw(context);
        }
    }
}
