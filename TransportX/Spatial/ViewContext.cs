using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Spatial
{
    public readonly struct ViewContext
    {
        public required Matrix4x4 View { get; init; }
        public required Matrix4x4 Projection { get; init; }
        public required BoundingFrustum Frustum { get; init; }
        public required WorldPose WorldPose { get; init; }
    }
}
