using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Bodies;
using TransportX.Cameras;
using TransportX.Rendering.Backend;
using TransportX.Spatial;
using Vortice.Direct3D11;

namespace TransportX.Rendering.Pipelines
{
    public static class SceneSubmitter
    {
        public static void SubmitBackground(this IRenderQueue renderQueue, ID3D11DeviceContext deviceContext, Camera camera, IEnumerable<TransformedModel> models)
        {
            TransformedDrawContext drawContext = new()
            {
                DeviceContext = deviceContext,
                RenderQueue = renderQueue,
                ChunkOffset = ChunkIndex.Zero,
                View = camera.View,
                Projection = camera.Projection,
                Frustum = camera.Frustum,
            };

            foreach (TransformedModel model in models)
            {
                model.Pose = new Pose(camera.WorldPose.Pose.Position);
                model.Draw(drawContext);
            }
        }

        public static void SubmitChunks(this IRenderQueue renderQueue, ID3D11DeviceContext deviceContext, Camera camera, ChunkCollection chunks, int drawChunkCount)
        {
            if (camera.VisibleLayers.HasFlag(Camera.VisualLayers.Normal)) Draw(RenderLayer.Normal);
            if (camera.VisibleLayers.HasFlag(Camera.VisualLayers.Colliders)) Draw(RenderLayer.Colliders);
            if (camera.VisibleLayers.HasFlag(Camera.VisualLayers.Network)) Draw(RenderLayer.Network);


            void Draw(RenderLayer layer)
            {
                for (int i = drawChunkCount - 1; 0 <= i; i--)
                {
                    for (int x = camera.WorldPose.Chunk.X - i; x <= camera.WorldPose.Chunk.X + i; x++)
                    {
                        int dz = int.Abs(x - camera.WorldPose.Chunk.X) == i ? 1 : i * 2;
                        for (int z = camera.WorldPose.Chunk.Z - i; z <= camera.WorldPose.Chunk.Z + i; z += dz)
                        {
                            ChunkIndex chunkIndex = new ChunkIndex(x, z);
                            if (chunks.TryGetValue(chunkIndex, out Chunk? chunk))
                            {
                                TransformedDrawContext drawContext = new()
                                {
                                    DeviceContext = deviceContext,
                                    RenderQueue = renderQueue,
                                    ChunkOffset = chunkIndex - camera.WorldPose.Chunk,
                                    View = camera.View,
                                    Projection = camera.Projection,
                                    Frustum = camera.Frustum,
                                    Layer = layer,
                                };
                                chunk!.Draw(drawContext);
                            }
                        }
                    }
                }
            }
        }

        public static void SubmitBodies(this IRenderQueue renderQueue, ID3D11DeviceContext deviceContext, Camera camera, IReadOnlyList<RigidBody> bodies)
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                RigidBody body = bodies[i];

                TransformedDrawContext drawContext = new()
                {
                    DeviceContext = deviceContext,
                    RenderQueue = renderQueue,
                    ChunkOffset = body.WorldPose.Chunk - camera.WorldPose.Chunk,
                    View = camera.View,
                    Projection = camera.Projection,
                    Frustum = camera.Frustum,
                    Layer = RenderLayer.Normal,
                };

                if (camera.VisibleLayers.HasFlag(Camera.VisualLayers.Normal))
                {
                    body.Draw(drawContext);
                }

                if (camera.VisibleLayers.HasFlag(Camera.VisualLayers.Colliders))
                {
                    drawContext = drawContext with
                    {
                        Layer = RenderLayer.Colliders,
                    };
                    body.Draw(drawContext);
                }

                if (camera.VisibleLayers.HasFlag(Camera.VisualLayers.Traffic))
                {
                    drawContext = drawContext with
                    {
                        Layer = RenderLayer.Traffic,
                    };
                    body.Draw(drawContext);
                }
            }
        }
    }
}
