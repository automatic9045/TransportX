using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Cameras;

namespace TransportX.Rendering.Shadows
{
    public class ShadowCamera : Camera
    {
        public ShadowCamera(int chunkCount) : base()
        {
            VisibleLayers = VisualLayers.Normal;
            DrawChunkCount = chunkCount;
        }

        public void LocateChunk(IWorldObject obj)
        {
            Locate(obj, Pose.Identity);
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
