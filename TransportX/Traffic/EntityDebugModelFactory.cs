using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Rendering;

namespace TransportX.Traffic
{
    public static class EntityDebugModelFactory
    {
        public static WireframeDebugModel CreateDebugModel(this ITrafficEntity entity, ID3D11Device device)
        {
            Vector3 min = new(-entity.Width / 2, 0, -entity.Length);
            Vector3 max = new(entity.Width / 2, entity.Height, 0);
            return WireframeDebugModel.CreateBoundingBox(device, Material.Default(), min, max);
        }
    }
}
