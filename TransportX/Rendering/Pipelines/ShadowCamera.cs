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
    public class ShadowCamera : Camera
    {
        public ShadowCamera() : base()
        {
            VisibleLayers = VisualLayers.Normal;
        }

        public void LocateChunk(ChunkIndex chunkIndex)
        {
            Locate(chunkIndex, Pose.Identity);
        }

        public void UpdateFromLight(Matrix4x4 lightView, Matrix4x4 lightProjection)
        {
            View = lightView;
            Projection = lightProjection;

            Matrix4x4 viewProj = lightView * lightProjection;
            Frustum = new BoundingFrustum(viewProj);
        }
    }
}
