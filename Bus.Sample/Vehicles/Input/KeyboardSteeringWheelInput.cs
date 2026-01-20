using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;

namespace Bus.Sample.Vehicles.Input
{
    internal class KeyboardSteeringWheelInput : SteeringWheelInput
    {
        private const float MinVehicleSpeed = 2.5f;
        private const float MaxVehicleSpeed = 20f;


        private readonly KeyObserver LeftKey;
        private readonly KeyObserver RightKey;
        private readonly KeyObserver ResetKey;
        private readonly Func<float> VehicleSpeedFunc;

        private bool IsResetting = false;

        public KeyboardSteeringWheelInput(KeyObserver leftKey, KeyObserver rightKey, KeyObserver resetKey, Func<float> vehicleSpeedFunc) : base()
        {
            LeftKey = leftKey;
            RightKey = rightKey;
            ResetKey = resetKey;
            VehicleSpeedFunc = vehicleSpeedFunc;
        }

        public override void Dispose()
        {
            LeftKey.Dispose();
            RightKey.Dispose();
            ResetKey.Dispose();
        }

        public override void Tick(TimeSpan elapsed)
        {
            IsResetting |= ResetKey.IsPressed;

            int rotateDirection = (LeftKey.IsPressed ? -1 : 0) + (RightKey.IsPressed ? 1 : 0);
            if (rotateDirection != 0) IsResetting = false;

            float vehicleSpeed = VehicleSpeedFunc();
            float rate = float.Clamp((vehicleSpeed - MinVehicleSpeed) / (MaxVehicleSpeed - MinVehicleSpeed), 0, 1);

            float speed = float.Lerp(2, 0.25f, rate);
            float resetSpeed = speed * 1.05f;

            if (IsResetting)
            {
                int rateSign = float.Sign(Rate);
                Rate -= rateSign * resetSpeed * (float)elapsed.TotalSeconds;
                if (float.Sign(Rate) != rateSign) Rate = 0;
            }
            else
            {
                Rate = float.Max(Min, float.Min(Rate + rotateDirection * speed * (float)elapsed.TotalSeconds, Max));
            }
        }
    }
}
