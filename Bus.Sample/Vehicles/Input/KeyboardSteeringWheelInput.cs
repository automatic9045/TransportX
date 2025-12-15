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
        private const float Speed = 1.25f;
        private const float ResetSpeed = Speed * 1.25f;


        private readonly KeyObserver LeftKey;
        private readonly KeyObserver RightKey;
        private readonly KeyObserver ResetKey;

        private bool IsResetting = false;

        public KeyboardSteeringWheelInput(KeyObserver leftKey, KeyObserver rightKey, KeyObserver resetKey) : base()
        {
            LeftKey = leftKey;
            RightKey = rightKey;
            ResetKey = resetKey;
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

            if (IsResetting)
            {
                int rateSign = float.Sign(Rate);
                Rate -= rateSign * ResetSpeed * (float)elapsed.TotalSeconds;
                if (float.Sign(Rate) != rateSign) Rate = 0;
            }
            else
            {
                Rate = float.Max(Min, float.Min(Rate + rotateDirection * Speed * (float)elapsed.TotalSeconds, Max));
            }
        }
    }
}
