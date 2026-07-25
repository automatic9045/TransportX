using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Cameras;
using TransportX.Spatial;

namespace TransportX.Rendering.Pipelines
{
    public class ShadowCamera : WorldObject
    {
        public ICamera.VisualLayers VisibleLayers { get; set; } = ICamera.VisualLayers.Normal;

        public ShadowCamera() : base()
        {
        }

        public void LocateChunk(ChunkIndex chunkIndex)
        {
            Locate(chunkIndex, Pose.Identity);
        }

        public ViewContext CreateViewContext(Matrix4x4 lightView, Matrix4x4 lightProjection)
        {
            return new ViewContext()
            {
                View = lightView,
                Projection = lightProjection,
                Frustum = new BoundingFrustum(lightView * lightProjection),
                WorldPose = WorldPose,
            };
        }
    }
}
