using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.XAudio2;

using TransportX.Mathematics;
using TransportX.Spatial;

namespace TransportX.Audio
{
    public class AudioClient : IAudioClient
    {
        private readonly Vector3LowPassFilter VelocityFilter = new(0.05f);

        public Listener Listener { get; }

        public AudioClient()
        {
            Listener = new Listener();
        }

        public void Reset(Vector3 velocity)
        {
            VelocityFilter.Reset(velocity);
            Listener.Velocity = velocity;
        }

        public void Tick(WorldPose worldPose, Vector3 velocity, TimeSpan elapsed)
        {
            Listener.OrientFront = worldPose.Pose.Direction;
            Listener.OrientTop = worldPose.Pose.Up;
            Listener.Position = worldPose.Pose.Position;
            Listener.Velocity = VelocityFilter.Next(velocity, elapsed);
        }
    }
}
