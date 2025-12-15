using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Bus.Common.Input;

namespace Bus.Sample.Vehicles.Input
{
    internal class KeyboardInput : IInput
    {
        public Slider Clutch { get; }
        public Slider Brake { get; }
        public Slider Throttle { get; }
        public SteeringWheelInput Steering { get; }
        public IATShifterInput ATShifter { get; }
        public IMTShifterInput MTShifter { get; }

        public KeyboardInput(Bus.Common.Input.InputManager inputManager)
        {
            Clutch = new SliderByKey(inputManager.ObserveKey(Key.LeftShift), 0.6f, 3, reverse: true);
            Brake = new SliderByKey(inputManager.ObserveKey(Key.Down), 1.2f, 1.5f);
            Throttle = new KeyboardThrottleInput(inputManager.ObserveKey(Key.Up), inputManager.ObserveKey(Key.OemBackslash));
            Steering = new KeyboardSteeringWheelInput(inputManager.ObserveKey(Key.Left), inputManager.ObserveKey(Key.Right), inputManager.ObserveKey(Key.OemQuestion));
            ATShifter = new KeyboardATShifterInput(
                inputManager.ObserveKey(Key.Q), inputManager.ObserveKey(Key.A), inputManager.ObserveKey(Key.Z),
                inputManager.ObserveKey(Key.W), inputManager.ObserveKey(Key.S), inputManager.ObserveKey(Key.X));
            MTShifter = new KeyboardMTShifterInput(
                inputManager.ObserveKey(Key.S), inputManager.ObserveKey(Key.X), inputManager.ObserveKey(Key.Z), inputManager.ObserveKey(Key.C));
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
