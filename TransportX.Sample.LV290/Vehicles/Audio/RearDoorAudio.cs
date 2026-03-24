using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;
using TransportX.Rendering;

using TransportX.Sample.Vehicles.Doors;

namespace TransportX.Sample.Vehicles.Audio
{
    internal class RearDoorAudio : AudioBase
    {
        private readonly PocketDoor Door;

        private readonly Sound3D OpenSound;
        private readonly Sound3D CloseSound;

        private bool OldIsOpen;

        public RearDoorAudio(SoundFactory soundFactory, PocketDoor door)
        {
            Door = door;
            OldIsOpen = Door.IsOpen;

            OpenSound = LoadSound("RearDoorOpen.wav");
            CloseSound = LoadSound("RearDoorClose.wav");


            Sound3D LoadSound(string filePath)
            {
                Sound3D sound = soundFactory.FromFile3D(filePath, new(-1.15f, 0.7f, -0.7f));

                sound.Emitter.CurveDistanceScaler = 40;
                sound.Emitter.InnerRadius = 1;
                sound.Emitter.InnerRadiusAngle = 0;

                return sound;
            }
        }

        public override void Dispose()
        {
            OpenSound.Dispose();
            CloseSound.Dispose();
        }

        public override void UpdateSound(Camera camera)
        {
            if (Door.IsOpen && !OldIsOpen)
            {
                OpenSound.Play(false);
                CloseSound.Stop();
            }
            else if (!Door.IsOpen && OldIsOpen)
            {
                OpenSound.Stop();
                CloseSound.Play(false);
            }
            OldIsOpen = Door.IsOpen;

            if (OpenSound.IsPlaying) OpenSound.Update(camera.Listener, camera.PlateX, camera.PlateZ);
            if (CloseSound.IsPlaying) CloseSound.Update(camera.Listener, camera.PlateX, camera.PlateZ);
        }
    }
}
