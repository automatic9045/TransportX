using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Bodies;
using TransportX.Rendering.Backend;
using TransportX.Spatial;

namespace TransportX.Rendering.Pipelines
{
    public static class SceneSubmitter
    {
        public static void SubmitBackground(this IRenderQueue renderQueue,
            ID3D11DeviceContext deviceContext, in ViewContext viewContext, IEnumerable<TransformedModel> models)
        {
            TransformedDrawContext drawContext = new()
            {
                DeviceContext = deviceContext,
                RenderQueue = renderQueue,
                ChunkOffset = ChunkIndex.Zero,
                ViewContext = viewContext,
            };

            foreach (TransformedModel model in models)
            {
                model.Pose = new Pose(viewContext.WorldPose.Pose.Position);
                model.Draw(drawContext);
            }
        }

        public static void SubmitChunks(this IRenderQueue renderQueue,
            ID3D11DeviceContext deviceContext, in ViewContext viewContext, ChunkCollection chunks, RenderLayer layer, int drawChunkCount)
        {
            for (int i = drawChunkCount - 1; 0 <= i; i--)
            {
                for (int x = viewContext.WorldPose.Chunk.X - i; x <= viewContext.WorldPose.Chunk.X + i; x++)
                {
                    int dz = int.Abs(x - viewContext.WorldPose.Chunk.X) == i ? 1 : i * 2;
                    for (int z = viewContext.WorldPose.Chunk.Z - i; z <= viewContext.WorldPose.Chunk.Z + i; z += dz)
                    {
                        ChunkIndex chunkIndex = new(x, z);
                        if (chunks.TryGetValue(chunkIndex, out Chunk? chunk))
                        {
                            TransformedDrawContext drawContext = new()
                            {
                                DeviceContext = deviceContext,
                                RenderQueue = renderQueue,
                                ChunkOffset = chunkIndex - viewContext.WorldPose.Chunk,
                                ViewContext = viewContext,
                                Layer = layer,
                            };
                            chunk!.Draw(drawContext);
                        }
                    }
                }
            }
        }

        public static void SubmitBodies(this IRenderQueue renderQueue,
            ID3D11DeviceContext deviceContext, in ViewContext viewContext, IReadOnlyList<RigidBody> bodies, RenderLayer layer)
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                RigidBody body = bodies[i];

                TransformedDrawContext drawContext = new()
                {
                    DeviceContext = deviceContext,
                    RenderQueue = renderQueue,
                    ChunkOffset = body.WorldPose.Chunk - viewContext.WorldPose.Chunk,
                    ViewContext = viewContext,
                    Layer = layer,
                };
                body.Draw(drawContext);
            }
        }
    }
}
