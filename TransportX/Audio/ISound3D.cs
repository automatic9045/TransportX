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
    public interface ISound3D : ISound, IMovable
    {
        public static new readonly ISound3D Empty = new Null();


        IMovable? AttachedTo { get; set; }

        void Tick(Listener listener, ChunkIndex cameraChunk, TimeSpan elapsed);


        private new class Null : ISound.Null, ISound3D
        {
            public IMovable? AttachedTo { get; set; } = null;

            public WorldPose WorldPose => WorldPose.Zero;
            public Vector3 Velocity => Vector3.Zero;
            public Vector3 AngularVelocity => Vector3.Zero;

            public event MovedEventHandler? Moved
            {
                add { }
                remove { }
            }

            public void Tick(Listener listener, ChunkIndex cameraChunk, TimeSpan elapsed)
            {
            }
        }
    }
}
