using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Audio;
using Bus.Common.Rendering;
using Vortice.XAudio2;

namespace Bus.Sample.Vehicles.Powertrain.Modules.Audio
{
    internal class EngineAudio : AudioModuleBase
    {
        private readonly Engine Engine;

        private readonly Sound3D LowSound;
        private readonly Sound3D HighSound;

        public float LowVolume { get; private set; } = 0;

        public EngineAudio(Engine engine, SoundFactory soundFactory)
        {
            Engine = engine;

            LowSound = LoadSound("EngineLow.wav");
            HighSound = LoadSound("EngineHigh.wav");


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
            LowSound.Dispose();
            HighSound.Dispose();
        }

        public override void UpdateSound(Camera camera)
        {
            LowVolume = float.Min(float.Max(0, -(Engine.Rpm - 1500) / 1500 + 1), 1);

            UpdateSoundState(camera, LowSound, LowVolume, 0.006f * Engine.AngularVelocity);
            UpdateSoundState(camera, HighSound, 1 - LowVolume, 0.0033f * Engine.AngularVelocity);
        }
    }
}
