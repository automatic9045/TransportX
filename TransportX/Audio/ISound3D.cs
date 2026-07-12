using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.XAudio2;

using TransportX.Spatial;

namespace TransportX.Audio
{
    public interface ISound3D : ISound, IWorldObject
    {
        public static new readonly ISound3D Empty = new Null();


        IWorldObject? AttachedTo { get; set; }

        void Update(Listener listener, ChunkIndex cameraChunk);


        private new class Null : ISound.Null, ISound3D
        {
            public WorldPose WorldPose { get; } = WorldPose.Zero;
            public Vector3 Velocity { get; } = Vector3.Zero;
            public IWorldObject? AttachedTo { get; set; } = null;

            public event MovedEventHandler? Moved
            {
                add { }
                remove { }
            }

            public void Update(Listener listener, ChunkIndex cameraChunk)
            {
            }
        }
    }
}
