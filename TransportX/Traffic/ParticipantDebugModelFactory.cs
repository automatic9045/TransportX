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
    public static class ParticipantDebugModelFactory
    {
        public static WireframeDebugModel CreateDebugModel(this ITrafficParticipant participant, ID3D11Device device)
        {
            Vector3 min = new(-participant.Width / 2, 0, -participant.Length);
            Vector3 max = new(participant.Width / 2, participant.Height, 0);
            return WireframeDebugModel.CreateBoundingBox(device, Material.Default(), min, max);
        }
    }
}
