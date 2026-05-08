using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

using TransportX.Input;

namespace TransportX.Sample.LV290.Vehicles.Input
{
    internal class KeyboardInput : IInput
    {
        public Slider Clutch { get; }
        public Slider Brake { get; }
        public Slider Throttle { get; }
        public SteeringWheelInput Steering { get; }
        public IATShifterInput ATShifter { get; }
        public IMTShifterInput MTShifter { get; }

        public IDoorSwitchInput DoorSwitch { get; }

        public KeyboardInput(TransportX.Input.InputManager inputManager, Func<float> vehicleSpeedFunc)
        {
            Clutch = new SliderByKey(inputManager.ObserveKey(Key.ShiftLeft), 0.6f, 3, reverse: true);
            Brake = new SliderByKey(inputManager.ObserveKey(Key.Down), 1.2f, 1.5f);
            Throttle = new KeyboardThrottleInput(inputManager.ObserveKey(Key.Up), inputManager.ObserveKey(Key.BackSlash));
            Steering = new KeyboardSteeringWheelInput(inputManager.ObserveKey(Key.Left), inputManager.ObserveKey(Key.Right), inputManager.ObserveKey(Key.Slash), vehicleSpeedFunc);
            ATShifter = new KeyboardATShifterInput(
                inputManager.ObserveKey(Key.Q), inputManager.ObserveKey(Key.A), inputManager.ObserveKey(Key.Z),
                inputManager.ObserveKey(Key.W), inputManager.ObserveKey(Key.S), inputManager.ObserveKey(Key.X));
            MTShifter = new KeyboardMTShifterInput(
                inputManager.ObserveKey(Key.S), inputManager.ObserveKey(Key.X), inputManager.ObserveKey(Key.Z), inputManager.ObserveKey(Key.C));

            DoorSwitch = new KeyboardDoorSwitchInput(inputManager.ObserveKey(Key.KeypadDivide), inputManager.ObserveKey(Key.KeypadMultiply));
        }

        public void Tick(TimeSpan elapsed)
        {
            Clutch.Tick(elapsed);
            Brake.Tick(elapsed);
            Throttle.Tick(elapsed);
            Steering.Tick(elapsed);
            MTShifter.Tick(elapsed);
        }
    }
}
