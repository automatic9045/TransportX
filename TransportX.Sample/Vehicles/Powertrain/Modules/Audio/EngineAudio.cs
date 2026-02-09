using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;
using TransportX.Rendering;

namespace TransportX.Sample.Vehicles.Powertrain.Modules.Audio
{
    internal class EngineAudio : AudioModuleBase
    {
        private readonly Engine Engine;

        private readonly Sound3D IdlingSound;
        private readonly Sound3D Sound;

        public float IdlingVolume { get; private set; } = 0;

        public EngineAudio(Engine engine, SoundFactory soundFactory)
        {
            Engine = engine;

            IdlingSound = LoadSound("EngineIdling.wav");
            Sound = LoadSound("Engine.wav");


            Sound3D LoadSound(string filePath)
            {
                Sound3D sound = soundFactory.FromFile3D(filePath, new(0, 0, -10));

                sound.Emitter.CurveDistanceScaler = 100;
                sound.Emitter.InnerRadius = 10;
                sound.Emitter.InnerRadiusAngle = float.Pi / 4;

                return sound;
            }
        }

        public override void Dispose()
        {
            IdlingSound.Dispose();
            Sound.Dispose();
        }

        public override void UpdateSound(Camera camera)
        {
            IdlingVolume = float.Clamp(-(Engine.Rpm - 600) / 100 + 1, 0, 1);

            UpdateSoundState(camera, IdlingSound, IdlingVolume, 0.0166f * Engine.AngularVelocity);
            UpdateSoundState(camera, Sound, 1, 0.0036f * Engine.AngularVelocity);
        }
    }
}
