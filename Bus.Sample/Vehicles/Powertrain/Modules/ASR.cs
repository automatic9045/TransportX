using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles.Powertrain.Modules
{
    internal class ASR
    {
        private static readonly float Threshold = 5;
        private static readonly float BrakeTorque = 1000;


        private readonly DriveWheel LeftWheel;
        private readonly DriveWheel RightWheel;

        public bool IsSlipping { get; private set; } = false;

        public ASR(DriveWheel leftWheel, DriveWheel rightWheel)
        {
            LeftWheel = leftWheel;
            RightWheel = rightWheel;
        }

        public void Tick(TimeSpan elapsed)
        {
            float deltaVelocity = LeftWheel.AngularVelocity - RightWheel.AngularVelocity;
            if (deltaVelocity < -Threshold)
            {
                IsSlipping = true;
                RightWheel.ApplyTorque(-float.Sign(RightWheel.AngularVelocity) * BrakeTorque, elapsed);
            }
            else if (Threshold < deltaVelocity)
            {
                IsSlipping = true;
                LeftWheel.ApplyTorque(-float.Sign(LeftWheel.AngularVelocity) * BrakeTorque, elapsed);
            }
            else
            {
                IsSlipping = false;
            }
        }
    }
}
