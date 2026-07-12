using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;
using TransportX.Cameras;

namespace TransportX.Sample.LV290.Vehicles.Audio
{
    internal abstract class AudioBase : IDisposable
    {
        public abstract void Dispose();
        public abstract void UpdateSound(Camera camera);

        protected void UpdateSoundState(Camera camera, Sound3D sound, float volume, float pitch)
        {
            if (!sound.IsPlaying && 0.001 <= volume)
            {
                sound.Volume = volume;
                sound.Pitch = pitch;
                sound.Play(true);
            }
            else if (sound.IsPlaying && volume < 0.001)
            {
                sound.Stop();
            }
            else
            {
                sound.Volume = volume;
                sound.Pitch = pitch;
            }

            sound.Update(camera.Listener, camera.WorldPose.Chunk);
        }
    }
}
