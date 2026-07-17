using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using TransportX.Communication;
using TransportX.Mathematics;

using TransportX.Scripting.Avatars;
using TransportX.Scripting.Avatars.Commands;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Chassis
{
    public class AxleFactory
    {
        private readonly ScriptAvatar Avatar;

        private readonly string Key;
        private readonly string ChildKeyPrefix;

        private PartInfo BeamInfo;
        private PartInfo WheelLInfo;
        private PartInfo WheelRInfo;

        private AxleCommand.KnuckleInfo KnuckleLInfo;
        private AxleCommand.KnuckleInfo JointRInfo;

        private AxleCommand.BrakeInfo BrakeLInfo;
        private AxleCommand.BrakeInfo BrakeRInfo;

        private AxleCommand.SteeringInfo SteeringL = default;
        private AxleCommand.SteeringInfo SteeringR = default;

        internal AxleFactory(ScriptAvatar avatar, string key)
        {
            Avatar = avatar;

            Key = key;
            ChildKeyPrefix = $"Axle_{Key}_";

            BeamInfo = PartInfo.Empty($"{ChildKeyPrefix}DefaultBeam");
            WheelLInfo = PartInfo.Empty($"{ChildKeyPrefix}DefaultWheelL");
            WheelRInfo = PartInfo.Empty($"{ChildKeyPrefix}DefaultWheelR");

            KnuckleLInfo = AxleCommand.KnuckleInfo.Default($"{ChildKeyPrefix}DefaultKnuckleL");
            JointRInfo = AxleCommand.KnuckleInfo.Default($"{ChildKeyPrefix}DefaultKnuckleR");

            BrakeLInfo = AxleCommand.BrakeInfo.Default($"{ChildKeyPrefix}DefaultBrakeL");
            BrakeRInfo = AxleCommand.BrakeInfo.Default($"{ChildKeyPrefix}DefaultBrakeR");
        }

        #region Beam
        public AxleFactory Beam(string partKey, string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double mass)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            BeamInfo = new PartInfo(partKey, modelKey, position.ToPose(), (float)mass);
            return this;
        }

        public AxleFactory Beam(string partKey, string modelKey, double x, double y, double z, double mass)
            => Beam(partKey, modelKey, x, y, z, 0, 0, 0, mass);

        public AxleFactory Beam(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double mass)
            => Beam($"{ChildKeyPrefix}Beam", modelKey, x, y, z, rotationX, rotationY, rotationZ, mass);

        public AxleFactory Beam(string modelKey, double x, double y, double z, double mass)
            => Beam($"{ChildKeyPrefix}Beam", modelKey, x, y, z, mass);
        #endregion

        #region Wheel
        public AxleFactory WheelL(string partKey, string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double mass)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            WheelLInfo = new PartInfo(partKey, modelKey, position.ToPose(), (float)mass);
            return this;
        }

        public AxleFactory WheelL(string partKey, string modelKey, double x, double y, double z, double mass)
            => WheelL(partKey, modelKey, x, y, z, 0, 0, 0, mass);

        public AxleFactory WheelL(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double mass)
            => WheelL($"{ChildKeyPrefix}WheelL", modelKey, x, y, z, rotationX, rotationY, rotationZ, mass);

        public AxleFactory WheelL(string modelKey, double x, double y, double z, double mass)
            => WheelL($"{ChildKeyPrefix}WheelL", modelKey, x, y, z, mass);

        public AxleFactory WheelR(string partKey, string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double mass)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            WheelRInfo = new PartInfo(partKey, modelKey, position.ToPose(), (float)mass);
            return this;
        }

        public AxleFactory WheelR(string partKey, string modelKey, double x, double y, double z, double mass)
            => WheelR(partKey, modelKey, x, y, z, 0, 0, 0, mass);

        public AxleFactory WheelR(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double mass)
            => WheelR($"{ChildKeyPrefix}WheelR", modelKey, x, y, z, rotationX, rotationY, rotationZ, mass);

        public AxleFactory WheelR(string modelKey, double x, double y, double z, double mass)
            => WheelR($"{ChildKeyPrefix}WheelR", modelKey, x, y, z, mass);
        #endregion

        #region Knuckle
        public AxleFactory KnuckleL(string jointKey, double springFrequency, double dampingRatio)
        {
            SpringSettings springSettings = new((float)springFrequency, (float)dampingRatio);
            KnuckleLInfo = new AxleCommand.KnuckleInfo(jointKey, springSettings);
            return this;
        }

        public AxleFactory KnuckleL(double springFrequency, double dampingRatio)
            => KnuckleL($"{ChildKeyPrefix}KnuckleL", springFrequency, dampingRatio);

        public AxleFactory KnuckleR(string jointKey, double springFrequency, double dampingRatio)
        {
            SpringSettings springSettings = new((float)springFrequency, (float)dampingRatio);
            JointRInfo = new AxleCommand.KnuckleInfo(jointKey, springSettings);
            return this;
        }

        public AxleFactory KnuckleR(double springFrequency, double dampingRatio)
            => KnuckleR($"{ChildKeyPrefix}KnuckleR", springFrequency, dampingRatio);
        #endregion

        #region Brake
        public AxleFactory BrakeL(string jointKey, Signal<float> signal, IReadOnlyCollection<CurvePoint> torqueCurvePoints)
        {
            BrakeLInfo = new AxleCommand.BrakeInfo(jointKey, signal, torqueCurvePoints);
            return this;
        }

        public AxleFactory BrakeL(string jointKey, string signalKey, IReadOnlyCollection<CurvePoint> torqueCurvePoints)
        {
            Signal<float> signal = Avatar.Commander.Signals.Float(signalKey);
            return BrakeL(jointKey, signal, torqueCurvePoints);
        }

        public AxleFactory BrakeL(string jointKey, Signal<float> signal, double maxTorque)
            => BrakeL(jointKey, signal, [(0, 0), (1, (float)maxTorque)]);

        public AxleFactory BrakeL(string jointKey, string signalKey, double maxTorque)
        {
            Signal<float> signal = Avatar.Commander.Signals.Float(signalKey);
            return BrakeL(jointKey, signal, maxTorque);
        }

        public AxleFactory BrakeL(Signal<float> signal, IReadOnlyCollection<CurvePoint> torqueCurvePoints)
            => BrakeL($"{ChildKeyPrefix}BrakeL", signal, torqueCurvePoints);

        public AxleFactory BrakeL(string signalKey, IReadOnlyCollection<CurvePoint> torqueCurvePoints)
            => BrakeL($"{ChildKeyPrefix}BrakeL", signalKey, torqueCurvePoints);

        public AxleFactory BrakeL(Signal<float> signal, double maxTorque)
            => BrakeL($"{ChildKeyPrefix}BrakeL", signal, maxTorque);

        public AxleFactory BrakeL(string signalKey, double maxTorque)
            => BrakeL($"{ChildKeyPrefix}BrakeL", signalKey, maxTorque);

        public AxleFactory BrakeR(string jointKey, Signal<float> signal, IReadOnlyCollection<CurvePoint> torqueCurvePoints)
        {
            BrakeRInfo = new AxleCommand.BrakeInfo(jointKey, signal, torqueCurvePoints);
            return this;
        }

        public AxleFactory BrakeR(string jointKey, string signalKey, IReadOnlyCollection<CurvePoint> torqueCurvePoints)
        {
            Signal<float> signal = Avatar.Commander.Signals.Float(signalKey);
            return BrakeR(jointKey, signal, torqueCurvePoints);
        }

        public AxleFactory BrakeR(string jointKey, Signal<float> signal, double maxTorque)
            => BrakeR(jointKey, signal, [(0, 0), (1, (float)maxTorque)]);

        public AxleFactory BrakeR(string jointKey, string signalKey, double maxTorque)
        {
            Signal<float> signal = Avatar.Commander.Signals.Float(signalKey);
            return BrakeR(jointKey, signal, maxTorque);
        }

        public AxleFactory BrakeR(Signal<float> signal, IReadOnlyCollection<CurvePoint> torqueCurvePoints)
            => BrakeR($"{ChildKeyPrefix}BrakeR", signal, torqueCurvePoints);

        public AxleFactory BrakeR(string signalKey, IReadOnlyCollection<CurvePoint> torqueCurvePoints)
            => BrakeR($"{ChildKeyPrefix}BrakeR", signalKey, torqueCurvePoints);

        public AxleFactory BrakeR(Signal<float> signal, double maxTorque)
            => BrakeR($"{ChildKeyPrefix}BrakeR", signal, maxTorque);

        public AxleFactory BrakeR(string signalKey, double maxTorque)
            => BrakeR($"{ChildKeyPrefix}BrakeR", signalKey, maxTorque);
        #endregion

        #region Steerable
        public AxleFactory SteerableL(Signal<float> signal, double leftAngle, double rightAngle)
        {
            SteeringL = new AxleCommand.SteeringInfo(signal, (float)leftAngle, (float)rightAngle);
            return this;
        }

        public AxleFactory SteerableL(string signalKey, double leftAngle, double rightAngle)
        {
            Signal<float> signal = Avatar.Commander.Signals.Float(signalKey);
            return SteerableL(signal, leftAngle, rightAngle);
        }

        public AxleFactory SteerableR(Signal<float> signal, double leftAngle, double rightAngle)
        {
            SteeringR = new AxleCommand.SteeringInfo(signal, (float)leftAngle, (float)rightAngle);
            return this;
        }

        public AxleFactory SteerableR(string signalKey, double leftAngle, double rightAngle)
        {
            Signal<float> signal = Avatar.Commander.Signals.Float(signalKey);
            return SteerableR(signal, leftAngle, rightAngle);
        }
        #endregion

        public AxleCommand Build()
        {
            DynamicPart beam = Avatar.Commander.Structure.Parts.Add(BeamInfo.PartKey, BeamInfo.ModelKey, BeamInfo.Pose)
                .GroupSkip()
                .BuildDynamic(BeamInfo.Mass);
            DynamicPart wheelL = Avatar.Commander.Structure.Parts.Add(WheelLInfo.PartKey, WheelLInfo.ModelKey, WheelLInfo.Pose)
                .BuildDynamic(WheelLInfo.Mass);
            DynamicPart wheelR = Avatar.Commander.Structure.Parts.Add(WheelRInfo.PartKey, WheelRInfo.ModelKey, WheelRInfo.Pose)
                .BuildDynamic(WheelRInfo.Mass);

            AxleCommand axle = AxleCommand.Create(Avatar, Key, beam, wheelL, wheelR, KnuckleLInfo, JointRInfo, BrakeLInfo, BrakeRInfo, SteeringL, SteeringR);
            Avatar.Commander.Component<Chassis>().AddAxle(axle);
            return axle;
        }


        private readonly record struct PartInfo(string PartKey, string ModelKey, Pose Pose, float Mass)
        {
            public static PartInfo Empty(string partKeyBase) => new($"{partKeyBase}_{Guid.NewGuid()}", string.Empty, Pose.Identity, 1);
        }
    }
}
