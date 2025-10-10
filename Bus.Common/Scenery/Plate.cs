using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scenery
{
    public class Plate
    {
        public static readonly int Size = 250;


        public List<LocatedModel> Models { get; } = new List<LocatedModel>();
        public List<NetworkElement> Network { get; } = new List<NetworkElement>();

        public Plate()
        {
        }

        public void Draw(ID3D11DeviceContext context, ID3D11Buffer constantBuffer, Matrix4x4 view, Matrix4x4 projection)
        {
            foreach (LocatedModel model in Models)
            {
                model.Draw(context, constantBuffer, view, projection);
            }

            foreach (NetworkElement element in Network)
            {
                element.Draw(context, constantBuffer, view, projection);
            }
        }
    }
}
