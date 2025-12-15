using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Audio;
using Bus.Common.Rendering;

namespace Bus.Sample.Vehicles.Powertrain.Modules.Audio
{
    internal abstract class AudioModuleBase : IDisposable
    {
        public abstract void Dispose();
        public abstract void UpdateSound(Camera camera);

        protected void UpdateSoundState(Camera camera, Sound3D sound, float volume, float pitch)
        {
            if (!sound.IsPlaying && 0.001 <= volume)
            {
                sound.SetVolume(volume);
                sound.SetPitch(pitch);
                sound.Play(true);
            }
            else if (sound.IsPlaying && volume < 0.001)
            {
                sound.Stop();
            }
            else
            {
                sound.SetVolume(volume);
                sound.SetPitch(pitch);
            }

            sound.Update(camera.Listener, camera.PlateX, camera.PlateZ);
        }
    }
}
