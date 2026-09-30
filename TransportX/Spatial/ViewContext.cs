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
        public required WorldPose WorldPose { get; init; }

        public float GetScreenRatio(in BoundingBox boundingBox, in Matrix4x4 world)
        {
            Vector3 extents = (boundingBox.Max - boundingBox.Min) * 0.5f;
            float radius = extents.Length();

            Vector3 center = (boundingBox.Min + boundingBox.Max) * 0.5f;
            Vector3 worldCenter = Vector3.Transform(center, world);

            Vector3 viewSpaceCenter = Vector3.Transform(worldCenter, View);
            if (viewSpaceCenter.Z + radius < 0.1f) return 0;

            float forwardDistance = float.Max(viewSpaceCenter.Z, 0.1f);
            float screenRatio = radius / forwardDistance * Projection.M22;
            return screenRatio;
        }
    }
}
