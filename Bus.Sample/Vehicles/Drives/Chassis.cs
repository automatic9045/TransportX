using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common;

using Bus.Sample.Vehicles.Interfaces;

namespace Bus.Sample.Vehicles.Drives
{
    internal class Chassis
    {
        private readonly ITransmission Transmission;

        private readonly Tire FrontLeftTire;
        private readonly Tire FrontRightTire;
        private readonly IReadOnlyList<Tire> RearLeftTires;
        private readonly IReadOnlyList<Tire> RearRightTires;

        public IReadOnlyList<Tire> Tires { get; }
        public double AverageAngularVelocity => Tires.Average(tire => tire.AngularVelocity);
        public double AverageVelocity => Tires.Average(tire => tire.Velocity);

        public Chassis(ITransmission transmission, Steering steering)
        {
            Transmission = transmission;

            float tireX = (float)(Spec.Width / 2 - Tire.Thickness * 0.5);
            float subTireX = (float)(Spec.Width / 2 - Tire.Thickness * 1.5);
            float frontTireZ = (float)-Spec.FrontOverhang;
            float rearTireZ = (float)(-Spec.FrontOverhang - Spec.Wheelbase);

            FrontLeftTire = new Tire(new Vector3(-tireX, 0, frontTireZ), Tire.SlipMode.Left, steering);
            FrontRightTire = new Tire(new Vector3(tireX, 0, frontTireZ), Tire.SlipMode.Right, steering);
            RearLeftTires = [
                new Tire(new Vector3(-tireX, 0, rearTireZ), Tire.SlipMode.None, steering),
                new Tire(new Vector3(-subTireX, 0, rearTireZ), Tire.SlipMode.None, steering)
            ];
            RearRightTires = [
                new Tire(new Vector3(tireX, 0, rearTireZ), Tire.SlipMode.None, steering),
                new Tire(new Vector3(subTireX, 0, rearTireZ), Tire.SlipMode.None, steering)
            ];

            Tires = [FrontLeftTire, FrontRightTire, ..RearLeftTires, ..RearRightTires];
        }

        public void ComputeTick(TimeSpan elapsed)
        {
            double inputTorque = Transmission.Torque / (RearLeftTires.Count + RearRightTires.Count);

            //foreach (Tire tire in RearLeftTires) tire.ComputeTick(inputTorque, vehicleVelocity, elapsed);
            //foreach (Tire tire in RearRightTires) tire.ComputeTick(inputTorque, vehicleVelocity, elapsed);
        }
    }
}
