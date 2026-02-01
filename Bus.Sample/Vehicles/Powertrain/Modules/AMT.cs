using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Interfaces;
using Bus.Sample.Vehicles.Powertrain.Physics;

namespace Bus.Sample.Vehicles.Powertrain.Modules
{
    internal class AMT : TransmissionBase
    {
        private static class Spec
        {
            public static readonly IReadOnlyList<float> GearRatios = [6.615f, 4.095f, 2.358f, 1.531f, 1, 0.722f];
            public static readonly float ReverseGearRatio = -6.615f;

            public static readonly IReadOnlyList<float> MinRpms = [0, 0, 500, 800, 900, 1000];
            public static readonly IReadOnlyList<float> MaxRpms = [2000, 2000, 1900, 1850, 1700, float.PositiveInfinity];
            public static readonly IReadOnlyList<float> MaxBoostRpms = [2400, 2400, 2300, 2200, 2100, float.PositiveInfinity];
        }

        private static class ShiftingDurations
        {
            public static readonly TimeSpan Phase0 = TimeSpan.FromSeconds(0.25f); // ラグ
            public static readonly TimeSpan Phase1 = TimeSpan.FromSeconds(0.25f); // スロットル1→0
            public static readonly TimeSpan Phase2 = TimeSpan.FromSeconds(0.25f); // クラッチ1→0
            public static readonly TimeSpan Phase3 = TimeSpan.FromSeconds(0.5f);  // ギア切断後ラグ
            public static readonly TimeSpan Phase4 = TimeSpan.FromSeconds(0.5f);  // ギア接続後ラグ
            public static readonly TimeSpan Phase5 = TimeSpan.FromSeconds(0.5f); // クラッチ0→1、スロットル0→1

            public static readonly TimeSpan UntilPhase1 = Phase0 + Phase1;
            public static readonly TimeSpan UntilPhase2 = Phase0 + Phase1 + Phase2;
            public static readonly TimeSpan UntilPhase3 = Phase0 + Phase1 + Phase2 + Phase3;
            public static readonly TimeSpan UntilPhase4 = Phase0 + Phase1 + Phase2 + Phase3 + Phase4;
            public static readonly TimeSpan UntilPhase5 = Phase0 + Phase1 + Phase2 + Phase3 + Phase4 + Phase5;
        }


        private readonly Engine Engine;
        private readonly FluidClutch Clutch;
        private readonly AMTShifter Shifter;
        private readonly Actuator ClutchActuatorKey;

        private int GearKey = 0;
        private float MinThrottleKey = 0;
        private float MaxThrottleKey = 1;

        private bool IsShifting = false;
        private int SrcGear = 0;
        private int DestGear = 0;
        private TimeSpan ShiftingElapsed = TimeSpan.Zero;

        protected override IReadOnlyList<float> GearRatios => Spec.GearRatios;
        protected override float ReverseGearRatio => Spec.ReverseGearRatio;

        public override int Gear => GearKey;
        public override float MinThrottle => MinThrottleKey;
        public override float MaxThrottle => MaxThrottleKey;
        public IAxis ClutchActuator => ClutchActuatorKey;

        public AMT(Engine engine, FluidClutch clutch, AMTShifter shifter, Actuator clutchActuator, Shaft input, Shaft output) : base(input, output)
        {
            Engine = engine;
            Clutch = clutch;
            Shifter = shifter;
            ClutchActuatorKey = clutchActuator;
        }

        public override void Tick(TimeSpan elapsed)
        {
            if (IsShifting) ShiftingElapsed += elapsed;

            int shifterGear;
            switch (Shifter.Position)
            {
                case AMTShifterPosition.R:
                    shifterGear = -1;
                    break;

                case AMTShifterPosition.N:
                    shifterGear = 0;
                    break;

                case AMTShifterPosition.D:
                    if (IsShifting)
                    {
                        shifterGear = DestGear;
                    }
                    else if (DestGear == 0)
                    {
                        shifterGear = 2;
                    }
                    else
                    {
                        shifterGear = DestGear;

                        float amount = float.Clamp((Engine.ECU.ThrottleInput - 0.8f) / 0.2f, 0, 1);
                        float rpm = Output.Rpm * GetGearRatio(Gear);

                        while (2 < shifterGear && rpm < Spec.MinRpms[shifterGear - 1])
                        {
                            rpm = rpm / GetGearRatio(shifterGear) * GetGearRatio(shifterGear - 1);
                            shifterGear--;
                        }

                        while (shifterGear < MaxGear && float.Lerp(Spec.MaxRpms[shifterGear - 1], Spec.MaxBoostRpms[shifterGear - 1], amount) < rpm)
                        {
                            rpm = rpm / GetGearRatio(shifterGear) * GetGearRatio(shifterGear + 1);
                            shifterGear++;
                        }
                    }
                    break;

                case AMTShifterPosition.M:
                    shifterGear = DestGear == 0 ? 2 : DestGear;
                    break;

                case AMTShifterPosition.Plus:
                    shifterGear = IsShifting || DestGear == MaxGear ? DestGear : DestGear + 1;
                    break;

                case AMTShifterPosition.Minus:
                    shifterGear = IsShifting || DestGear == 1 ? DestGear : DestGear - 1;
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (shifterGear != DestGear)
            {
                IsShifting = true;
                SrcGear = Gear;
                DestGear = shifterGear;

                if (DestGear == 0)
                {
                    ShiftingElapsed = ShiftingDurations.UntilPhase3;
                }
                else if (ShiftingDurations.UntilPhase2 < ShiftingElapsed || Gear == 0 || (0 < DestGear && DestGear < Gear) || Engine.ECU.Throttle < 0.05f)
                {
                    ShiftingElapsed = ShiftingDurations.UntilPhase2;
                }
            }

            float minThrottle = 0;
            float maxThrottle = 1;
            float clutchRate = 1;
            bool immediateLockup = false;
            if (IsShifting)
            {
                if (ShiftingElapsed < ShiftingDurations.Phase0)
                {
                    maxThrottle = Engine.ECU.ThrottleInput * 0.625f;
                }
                else if (ShiftingElapsed < ShiftingDurations.UntilPhase1)
                {
                    maxThrottle = float.Lerp(0.5f, 0, (float)((ShiftingElapsed - ShiftingDurations.Phase0) / ShiftingDurations.Phase1));
                }
                else if (ShiftingElapsed < ShiftingDurations.UntilPhase2)
                {
                    maxThrottle = 0;
                    clutchRate = float.Lerp(1, 0, (float)((ShiftingElapsed - ShiftingDurations.UntilPhase1) / ShiftingDurations.Phase2));
                }
                else if (ShiftingElapsed < ShiftingDurations.UntilPhase3)
                {
                    maxThrottle = 0;
                    clutchRate = 0;
                    GearKey = 0;
                }
                else if (ShiftingElapsed < ShiftingDurations.UntilPhase4)
                {
                    maxThrottle = 0;
                    clutchRate = 0;
                    GearKey = DestGear;
                }
                else if (ShiftingElapsed < ShiftingDurations.UntilPhase5)
                {
                    float amount = (float)((ShiftingElapsed - ShiftingDurations.UntilPhase4) / ShiftingDurations.Phase5);
                    maxThrottle = float.Lerp(0, 1, amount);
                    if (0 < DestGear && DestGear < SrcGear && 100 < Output.Rpm)
                    {
                        minThrottle = float.Min(0.75f, float.Lerp(2, 0, amount));
                        maxThrottle = float.Max(0.75f, maxThrottle);
                    }
                    clutchRate = float.Max(0, float.Lerp(-0.5f, 1, amount));
                    GearKey = DestGear;
                    immediateLockup = true;
                }
                else
                {
                    GearKey = DestGear;
                    IsShifting = false;
                    ShiftingElapsed = TimeSpan.Zero;
                }
            }

            bool lockup = Clutch.Lockup;
            float finalClutchRate = clutchRate;
            {
                float rpm = Output.Rpm * GetGearRatio(Gear);
                if (Gear == 0)
                {
                    lockup = false;
                    finalClutchRate = 0;
                }
                else if (immediateLockup)
                {
                    lockup = true;
                }
                else if (Clutch.Lockup)
                {
                    if (rpm < 600) lockup = false;
                }
                else
                {
                    float lockupRpm = float.Clamp(float.Lerp(700, 1100, (Engine.ECU.ThrottleInput - 0.7f) / 0.3f), 700, 1100);
                    if (lockupRpm < rpm) lockup = true;
                }

                if (Clutch.LockupRate == 0)
                {
                    maxThrottle = float.Max(minThrottle, maxThrottle * 0.6f);
                }
                else if (Clutch.LockupRate < 1)
                {
                    maxThrottle = float.Max(minThrottle, maxThrottle * 0.32f);
                }

                if (!IsShifting && Engine.Rpm < 550)
                {
                    finalClutchRate = float.Clamp((Engine.Rpm - 500) / 50, 0, finalClutchRate);
                }
            }

            MinThrottleKey = minThrottle;
            MaxThrottleKey = maxThrottle;
            Clutch.EnableCreep = Gear <= 3;
            Clutch.Lockup = lockup;
            Clutch.LockupMode = immediateLockup ? FluidClutch.LockupResponseMode.Immediate : FluidClutch.LockupResponseMode.Normal;
            ClutchActuatorKey.Rate = finalClutchRate;

            base.Tick(elapsed);
        }
    }
}
