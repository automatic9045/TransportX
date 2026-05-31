using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Bodies
{
    public class BodyCollection : List<RigidBody>, IDisposable
    {
        public BodyCollection() : base()
        {
        }

        public void Dispose()
        {
            foreach (RigidBody body in this) body.Dispose();
        }

        public void SetCameraPosition(WorldPose cameraWorldPose, int computeChunkCount)
        {
            foreach (RigidBody body in this)
            {
                ChunkIndex fromCamera = body.WorldPose.Chunk - cameraWorldPose.Chunk;
                body.SetFromCamera(fromCamera);
            }
        }

        public void SubTick(TimeSpan elapsed, WorldPose cameraWorldPose, int computeChunkCount)
        {
            foreach (RigidBody body in this)
            {
                body.SubTick(elapsed);
                ChunkIndex fromCamera = body.WorldPose.Chunk - cameraWorldPose.Chunk;
                body.SetFromCamera(fromCamera);
            }
        }

        public void Tick(TimeSpan elapsed)
        {
            foreach (RigidBody body in this) body.Tick(elapsed);
        }
    }
}
