using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.XAudio2;

using Bus.Common;
using Bus.Common.Audio;

namespace Bus.Sample.Vehicles.Drives
{
    internal class Engine : IEngine, IDisposable
    {
        private readonly double Inertia = 10;

        private readonly Diagram Performance = new Diagram([
            new DiagramPoint(0, 0),
            new DiagramPoint(990, 626),
            new DiagramPoint(1200, 700),
            new DiagramPoint(1390, 734),
            new DiagramPoint(1950, 735),
            new DiagramPoint(2415, 700),
            new DiagramPoint(3775, 0),
        ]);

        private readonly AttachableObject Location;

        private readonly Sound3D LowSound;
        private readonly Sound3D MiddleSound;
        private readonly Sound3D HighSound;

        public double Torque { get; private set; } = 0;
        public double AngularVelocity { get; private set; } = 600d / 60 * (2 * double.Pi);
        public double Rpm => AngularVelocity / (2 * double.Pi) * 60;

        public double LowVolume { get; private set; } = 0;

        public Engine(SoundFactory soundFactory, LocatableObject body)
        {
            Location = new AttachableObject(body, Matrix4x4.CreateTranslation(0, 0,-10));

            LowSound = LoadSound("EngineLow.wav");
            MiddleSound = LoadSound("EngineMiddle.wav");
            HighSound = LoadSound("EngineHigh.wav");


            Sound3D LoadSound(string filePath)
            {
                Sound3D sound = soundFactory.FromFile3D(filePath, Location);

                sound.Emitter.CurveDistanceScaler = 100;
                sound.Emitter.InnerRadius = 10;
                sound.Emitter.InnerRadiusAngle = float.Pi / 4;

                return sound;
            }
        }

        public void Dispose()
        {
            LowSound.Dispose();
            MiddleSound.Dispose();
            HighSound.Dispose();
        }

        public void Accelerate(double torque, TimeSpan elapsed)
        {
            double acceleration = torque / Inertia;
            AngularVelocity += acceleration * elapsed.TotalSeconds;
        }

        public void ComputeTick(double throttle, TimeSpan elapsed)
        {
            double fullTorque = Performance.GetValue(Rpm);
            double resistance = 0 * AngularVelocity;
            Torque = fullTorque * throttle - resistance;

            Accelerate(Torque, elapsed);
        }

        public void UpdateSound(X3DAudio x3dAudio, Listener listener)
        {
            LowVolume = double.Min(double.Max(0, -(Rpm - 1500) / 1500 + 1), 1);

            UpdateSoundState(LowSound, LowVolume, 0.006 * AngularVelocity);
            UpdateSoundState(MiddleSound, 0, 0.0065 * AngularVelocity);
            UpdateSoundState(HighSound, 1 - LowVolume, 0.0033 * AngularVelocity);


            void UpdateSoundState(Sound3D sound, double volume, double pitch)
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

                sound.Update(x3dAudio, listener);
            }
        }
    }
}
