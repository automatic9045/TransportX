using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using TransportX.Communication;
using TransportX.Mathematics;

using TransportX.Scripting.Avatars;
using TransportX.Scripting.Avatars.Commands;

using TransportX.Domains.RoadVehicles.Chassis;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Chassis
{
    public class AxleCommand
    {
        private readonly ScriptAvatar Avatar;

        public string Key { get; }
        public Axle Axle { get; }

        public required DynamicPart Beam { get; init; }
        public required DynamicPart WheelL { get; init; }
        public required DynamicPart WheelR { get; init; }

        public required KnuckleCommand KnuckleL { get; init; }
        public required KnuckleCommand KnuckleR { get; init; }

        public required BrakeCommand BrakeL { get; init; }
        public required BrakeCommand BrakeR { get; init; }

        public AxleCommand(ScriptAvatar avatar, string key, Axle axle)
        {
            Avatar = avatar;
            Key = key;
            Axle = axle;
        }

        public static AxleCommand Create(ScriptAvatar avatar, string key, DynamicPart beam, DynamicPart wheelL, DynamicPart wheelR,
            KnuckleInfo knuckleL, KnuckleInfo knuckleR, BrakeInfo brakeL, BrakeInfo brakeR, SteeringInfo steeringL, SteeringInfo steeringR)
        {
            Axle axle = Axle.Create(avatar.PhysicsHost.Simulation, beam.Model,
                CreateWheel(wheelL, knuckleL, steeringL, brakeL), CreateWheel(wheelR, knuckleR, steeringR, brakeR));

            return new AxleCommand(avatar, key, axle)
            {
                Beam = beam,
                WheelL = wheelL,
                WheelR = wheelR,

                KnuckleL = new KnuckleCommand(avatar, axle.KnuckleL, beam, wheelL, knuckleL.JointKey),
                KnuckleR = new KnuckleCommand(avatar, axle.KnuckleR, beam, wheelR, knuckleR.JointKey),

                BrakeL = new BrakeCommand(avatar, axle.BrakeL, beam, wheelL, brakeL.JointKey),
                BrakeR = new BrakeCommand(avatar, axle.BrakeR, beam, wheelR, brakeR.JointKey),
            };


            static Axle.Wheel CreateWheel(DynamicPart part, KnuckleInfo joint, SteeringInfo steering, BrakeInfo brake)
            {
                return new Axle.Wheel(part.Model, joint.SpringSettings,
                    steering.Signal, steering.LeftAngle, steering.RightAngle,
                    brake.Signal, new Curve(brake.TorqueCurvePoints));
            }
        }

        internal static AxleCommand InvalidEmpty(ScriptAvatar avatar, string key)
        {
            return Create(avatar, key,
                DynamicPart.InvalidEmpty(avatar, key), DynamicPart.InvalidEmpty(avatar, key), DynamicPart.InvalidEmpty(avatar, key),
                KnuckleInfo.Default(key), KnuckleInfo.Default(key), BrakeInfo.Default(key), BrakeInfo.Default(key),
                new SteeringInfo(null, 0, 0), new SteeringInfo(null, 0, 0));
        }


        public readonly record struct KnuckleInfo(string JointKey, SpringSettings SpringSettings)
        {
            internal static KnuckleInfo Default(string partKeyBase) => new($"{partKeyBase}_{Guid.NewGuid()}", new SpringSettings(30, 1));
        }

        public readonly record struct BrakeInfo(string JointKey, Signal<float>? Signal, IReadOnlyCollection<CurvePoint> TorqueCurvePoints)
        {
            internal static BrakeInfo Default(string partKeyBase) => new($"{partKeyBase}_{Guid.NewGuid()}", null, [(0, 0)]);
        }

        public readonly record struct SteeringInfo(Signal<float>? Signal, float LeftAngle, float RightAngle);
    }
}
