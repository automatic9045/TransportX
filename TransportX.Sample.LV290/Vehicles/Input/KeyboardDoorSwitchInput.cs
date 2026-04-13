using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

namespace TransportX.Sample.LV290.Vehicles.Input
{
    internal class KeyboardDoorSwitchInput : IDoorSwitchInput
    {
        private readonly KeyObserver FrontKey;
        private readonly KeyObserver RearKey;

        public bool IsFrontOpen { get; private set; }
        public bool IsRearOpen { get; private set; }

        public KeyboardDoorSwitchInput(KeyObserver frontKey, KeyObserver rearKey)
        {
            FrontKey = frontKey;
            RearKey = rearKey;

            FrontKey.Pressed += (sender, e) => IsFrontOpen = !IsFrontOpen;
            RearKey.Pressed += (sender, e) => IsRearOpen = !IsRearOpen;
        }
    }
}
