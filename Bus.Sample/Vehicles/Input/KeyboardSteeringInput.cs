using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;

namespace Bus.Sample.Vehicles.Input
{
    internal class KeyboardSteeringInput : SteeringInput
    {
        private const double DriverTorque = 5;
        private const double DampingCoeff = 1.5;


        private readonly KeyObserver LeftKey;
        private readonly KeyObserver RightKey;

        public KeyboardSteeringInput(KeyObserver leftKey, KeyObserver rightKey) : base()
        {
            LeftKey = leftKey;
            RightKey = rightKey;
        }

        public override void Dispose()
        {
            LeftKey.Dispose();
            RightKey.Dispose();
        }

        public override void Tick(TimeSpan elapsed)
        {
            int rotateDirection = (LeftKey.IsPressed ? -1 : 0) + (RightKey.IsPressed ? 1 : 0);
            double torque = DriverTorque * rotateDirection - SteeringTorque;
            Rate = double.Max(0, double.Min(Rate + torque / DampingCoeff * elapsed.TotalSeconds, 1));
        }
    }
}
