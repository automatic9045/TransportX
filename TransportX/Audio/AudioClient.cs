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
    public class AudioClient : IAudioClient
    {
        public Listener Listener { get; }

        public AudioClient()
        {
            Listener = new Listener();
        }

        public void Update(WorldPose worldPose, Vector3 velocity)
        {
            Listener.OrientFront = worldPose.Pose.Direction;
            Listener.OrientTop = worldPose.Pose.Up;
            Listener.Position = worldPose.Pose.Position;
            Listener.Velocity = velocity;
        }
    }
}
