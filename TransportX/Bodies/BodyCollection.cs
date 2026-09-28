using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Bodies
{
    public class BodyCollection : IBodyCollection, IDisposable
    {
        private readonly List<RigidBody> Bodies = [];

        public RigidBody this[int index] => Bodies[index];
        public int Count => Bodies.Count;

        public event Action<RigidBody>? Added;
        public event Action<RigidBody>? Removed;

        public BodyCollection() : base()
        {
        }

        public void Dispose()
        {
            foreach (RigidBody body in this) body.Dispose();
        }

        public void Add(RigidBody body)
        {
            Bodies.Add(body);
            Added?.Invoke(body);
        }

        public bool Remove(RigidBody body)
        {
            if (Bodies.Remove(body))
            {
                Removed?.Invoke(body);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetCameraChunk(ChunkIndex cameraChunk)
        {
            foreach (RigidBody body in this)
            {
                ChunkIndex fromCamera = body.WorldPose.Chunk - cameraChunk;
                body.SetFromCamera(fromCamera);
            }
        }

        public void SubTick(TimeSpan elapsed, ChunkIndex cameraChunk, int computeChunkCount)
        {
            foreach (RigidBody body in this)
            {
                body.SubTick(elapsed);
            }
        }

        public void Tick(TimeSpan elapsed)
        {
            foreach (RigidBody body in this) body.Tick(elapsed);
        }

        public IEnumerator<RigidBody> GetEnumerator() => Bodies.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
