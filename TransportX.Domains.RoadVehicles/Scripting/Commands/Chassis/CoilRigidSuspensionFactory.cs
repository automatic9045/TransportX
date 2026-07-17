using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using TransportX.Diagnostics;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;
using TransportX.Scripting.Avatars.Commands;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Chassis
{
    public class CoilRigidSuspensionFactory
    {
        private readonly ScriptAvatar Avatar;

        private readonly string KeyBase;
        private readonly string BodyPartKey;
        private readonly string AxleKey;

        private float SpringFrequency = 5;

        internal CoilRigidSuspensionFactory(ScriptAvatar avatar, string keyBase, string bodyPartKey, string axleKey)
        {
            Avatar = avatar;

            KeyBase = keyBase;
            BodyPartKey = bodyPartKey;
            AxleKey = axleKey;
        }

        public CoilRigidSuspensionFactory Spring(double frequency)
        {
            SpringFrequency = (float)frequency;
            return this;
        }

        public void Build()
        {
            if (!Avatar.Commander.Structure.Parts.All.GetValue(BodyPartKey, out Part body)) return;
            if (body is not DynamicPart bodyPart)
            {
                ScriptError error = new(ErrorLevel.Error, "車体にダイナミックでないパーツを指定することはできません。");
                Avatar.ErrorCollector.Report(error);
                return;
            }

            if (!Avatar.Commander.Component<Chassis>().Axles.GetValue(AxleKey, out AxleCommand axle)) return;

            Pose baseToBody = bodyPart.Model.BaseToCollider;
            Pose baseToAxle = axle.Beam.Model.BaseToCollider;
            Pose axleToBase = axle.Beam.Model.ColliderToBase;

            Vector3 axisX = Pose.TransformNormal(Vector3.UnitX, baseToBody);
            Vector3 axisY = Pose.TransformNormal(Vector3.UnitY, baseToBody);
            Vector3 axisZ = Pose.TransformNormal(Vector3.UnitZ, baseToBody);

            Vector3 axleInBody = (axleToBase * baseToBody).Position;
            Vector3 springOffsetInAxle = Pose.TransformNormal(Vector3.UnitX, baseToAxle);

            LinearAxisServo lateralRod = new()
            {
                LocalOffsetA = axleInBody,
                LocalOffsetB = Vector3.Zero,
                LocalPlaneNormal = axisX,
                ServoSettings = ServoSettings.Default,
                SpringSettings = new SpringSettings(30, 1),
            };
            Avatar.Commander.Structure.Joints.Add($"{KeyBase}_LateralRod", bodyPart, axle.Beam, lateralRod);

            LinearAxisServo trailingArm = new()
            {
                LocalOffsetA = axleInBody,
                LocalOffsetB = Vector3.Zero,
                LocalPlaneNormal = axisZ,
                ServoSettings = ServoSettings.Default,
                SpringSettings = new SpringSettings(30, 1),
            };
            Avatar.Commander.Structure.Joints.Add($"{KeyBase}_TrailingArm", bodyPart, axle.Beam, trailingArm);

            LinearAxisServo springL = new()
            {
                LocalPlaneNormal = axisY,
                TargetOffset = 0,
                LocalOffsetA = axleInBody - axisX,
                LocalOffsetB = -springOffsetInAxle,
                ServoSettings = ServoSettings.Default,
                SpringSettings = new SpringSettings(SpringFrequency, 1),
            };
            Avatar.Commander.Structure.Joints.Add($"{KeyBase}_SpringL", bodyPart, axle.Beam, springL);

            LinearAxisServo springR = new()
            {
                LocalPlaneNormal = axisY,
                TargetOffset = 0,
                LocalOffsetA = axleInBody + axisX,
                LocalOffsetB = springOffsetInAxle,
                ServoSettings = ServoSettings.Default,
                SpringSettings = new SpringSettings(SpringFrequency, 1),
            };
            Avatar.Commander.Structure.Joints.Add($"{KeyBase}_SpringR", bodyPart, axle.Beam, springR);

            AngularHinge stabilizer = new()
            {
                LocalHingeAxisA = axisZ,
                LocalHingeAxisB = Pose.TransformNormal(Vector3.UnitZ, baseToAxle),
                SpringSettings = new SpringSettings(15, 1.5f),
            };
            Avatar.Commander.Structure.Joints.Add($"{KeyBase}_Stabilizer", bodyPart, axle.Beam, stabilizer);
        }
    }
}
