using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.XAudio2;

using Bus.Common.Audio;
using Bus.Common;
using Bus.Sample.Vehicles.Interfaces;

namespace Bus.Sample.Vehicles.Drives
{
    internal class AT : TransmissionBase
    {
        public const int MaxGear = 6;


        protected override IReadOnlyList<double> GearRatios { get; } = [3.486, 1.864, 1.409, 1, 0.749, 0.652];
        protected override IReadOnlyList<double> ReverseGearRatios { get; } = [-5.027];
        protected override double FinalRatio { get; } = 6.5;

        private readonly IReadOnlyList<double> MinRpms = [0, 450, 1000, 1000, 1000, 1000];
        private readonly IReadOnlyList<double> MaxRpms = [1500, 1400, 1500, 1450, 1400, 0];
        private readonly IReadOnlyList<double> KickdownRpms = [1300, 1300, 1400, 1400, 1300, 0];

        private readonly AttachableObject Location;

        private readonly ATShifter Shifter;

        private readonly Sound3D Sound;
        private readonly IReadOnlyList<double> SoundPitchRatios = [0.001, 0.00065, 0.00038, 0, 0.001, 0.001149];

        private TimeSpan SinceGearChanged = TimeSpan.Zero;
        private bool IsReadyToStop = false;

        private double TargetSoundPitch = 0;
        private double SoundPitch = 0;

        private double OldThrottle = 0;

        public TorqueConverter TorqueConverter { get; }
        public override string PositionText => "";
        public override Vector2 LeverCoord { get; } = Vector2.Zero;

        private double _Torque = 0;
        public override double Torque => _Torque;

        public AT(Engine engine, DoorInterlock interlock, ATShifter shifter, SoundFactory soundFactory, LocatableObject body) : base(engine, interlock)
        {
            Location = new AttachableObject(body, Matrix4x4.CreateTranslation(0, 0, -10));
            TorqueConverter = new TorqueConverter(engine);

            Shifter = shifter;
            Sound = LoadSound("AT.wav");


            Sound3D LoadSound(string filePath)
            {
                Sound3D sound = soundFactory.FromFile3D(filePath, Location);

                sound.Emitter.CurveDistanceScaler = 100;
                sound.Emitter.InnerRadius = 10;
                sound.Emitter.InnerRadiusAngle = float.Pi / 4;

                return sound;
            }
        }

        public override void Dispose()
        {
            Sound.Dispose();
        }

        public override void ComputeTick(double vehicleAngularVelocity, TimeSpan elapsed)
        {
            switch (Shifter.Position)
            {
                case -1:
                    Gear = -1;
                    IsReadyToStop = false;
                    break;

                case 0:
                    Gear = 0;
                    IsReadyToStop = false;
                    break;

                case MaxGear:
                    if (Gear <= 0) Gear = 1;
                    break;
            }

            double throttle = Interlock.ThrottleRate;

            SinceGearChanged += elapsed;
            if (IsReadyToStop && (vehicleAngularVelocity < 1 || 0 < throttle)) IsReadyToStop = false;
            
            if (500 < SinceGearChanged.TotalMilliseconds)
            {
                double theoreticalRpm = GetTheoreticalRpm(vehicleAngularVelocity, Gear);
                if (2 <= Gear && theoreticalRpm < MinRpms[Gear - 1])
                {
                    IsReadyToStop = Gear == 2;
                    Gear--;
                    SinceGearChanged = TimeSpan.Zero;
                }
                else if (1 <= Gear && Gear < MaxGear && MaxRpms[Gear - 1] < theoreticalRpm)
                {
                    Gear++;
                    SinceGearChanged = TimeSpan.Zero;
                }
                else if (2 <= Gear && OldThrottle < 0.4 && 0.4 <= throttle && GetTheoreticalRpm(vehicleAngularVelocity, Gear - 1) < KickdownRpms[Gear - 2])
                {
                    Gear--;
                    SinceGearChanged = TimeSpan.Zero;
                }
            }

            double clutch = Gear == 0 || Interlock.Modes.HasFlag(DoorInterlockModes.Brake) || IsReadyToStop ? 0 : Engine.Rpm < 580 ? (1 - Interlock.BrakeRate) * 0.5 : Gear == 2 && 0 < throttle ? 0.8 : 1;
            double idleThrottle = Gear == 0 ? (Engine.Rpm < 595 ? 1 : Engine.Rpm < 600 ? (600 - Engine.Rpm) / 5 : 0)
                    : Engine.Rpm < 575 ? 1 : Engine.Rpm < 580 ? (580 - Engine.Rpm) / 5 : 0;
            if (Engine.Rpm < 580) throttle = double.Min(throttle * 2, 1);

            double ratio = GetRatio(Gear);

            TorqueConverter.ComputeTick(vehicleAngularVelocity / ratio, double.Max(idleThrottle, throttle), elapsed);
            double torque = TorqueConverter.Torque * clutch * ratio;

            //Engine.AngularVelocity += 0.3 * Engine.Torque * (1 - clutch) * elapsed.TotalSeconds;

            if (Gear != 0)
            {
                /*
                double angularVelocity = vehicleAngularVelocity * ratio;
                double targetVelocity = Engine.AngularVelocity * Spec.EngineInertiaRatio + angularVelocity * (1 - Spec.EngineInertiaRatio);
                double velocity = (Gear < 0 ? 100 : Gear <= 2 ? 60 : 50) * elapsed.TotalSeconds;
                torque += clutch * double.Sign(targetVelocity - angularVelocity) * double.Min(double.Abs(targetVelocity - angularVelocity), velocity) / ratio / elapsed.TotalSeconds;
                Engine.AngularVelocity += clutch * double.Sign(targetVelocity - Engine.AngularVelocity) * double.Min(double.Abs(targetVelocity - Engine.AngularVelocity), velocity);
                */

            }

            TargetSoundPitch = 0 < Gear ? SoundPitchRatios[Gear - 1] * GetTheoreticalRpm(vehicleAngularVelocity, Gear) : 0;
            SoundPitch += 2 * double.Sign(TargetSoundPitch - SoundPitch) * elapsed.TotalSeconds;

            OldThrottle = throttle;
            _Torque = torque;
        }

        public override void UpdateSound(X3DAudio x3dAudio, Listener listener)
        {
            Sound.SetVolume(0.5);
            Sound.SetPitch(SoundPitch);

            if (!Sound.IsPlaying && 0.001 < SoundPitch) Sound.Play(true);

            Sound.Update(x3dAudio, listener);

            Engine.UpdateSound(x3dAudio, listener);
        }
    }
}
