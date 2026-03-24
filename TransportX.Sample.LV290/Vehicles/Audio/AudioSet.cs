using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

using TransportX.Sample.Vehicles.Doors;
using TransportX.Sample.Vehicles.Powertrain.Modules;

namespace TransportX.Sample.Vehicles.Audio
{
    internal class AudioSet : IDisposable
    {
        private readonly AudioBase[] Audios;

        public AudioSet(SoundFactory soundFactory, Engine engine, BifoldDoor frontDoor, PocketDoor rearDoor)
        {
            EngineAudio engineAudio = new(soundFactory, engine);

            FrontDoorAudio frontDoorAudio = new(soundFactory, frontDoor);
            RearDoorAudio rearDoorAudio = new(soundFactory, rearDoor);

            Audios = [
                engineAudio, frontDoorAudio, rearDoorAudio,
            ];
        }

        public void Dispose()
        {
            foreach (AudioBase audio in Audios)
            {
                audio.Dispose();
            }
        }

        public void UpdateSound(Camera camera)
        {
            foreach (AudioBase audio in Audios)
            {
                audio.UpdateSound(camera);
            }
        }
    }
}
