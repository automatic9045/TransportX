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
        public SteeringInput Steering { get; }
        public IATShifterInput ATShifter { get; }

        public KeyboardInput(Bus.Common.Input.InputManager inputManager)
        {
            Clutch = new SliderByKey(inputManager.ObserveKey(Key.LeftShift), 3, 0.6);
            Brake = new SliderByKey(inputManager.ObserveKey(Key.Down), 1.2, 1.5);
            Throttle = new KeyboardThrottleInput(inputManager.ObserveKey(Key.Up), inputManager.ObserveKey(Key.OemBackslash));
            Steering = new KeyboardSteeringInput(inputManager.ObserveKey(Key.Left), inputManager.ObserveKey(Key.Right));
            ATShifter = new KeyboardATShifterInput(
                inputManager.ObserveKey(Key.Q), inputManager.ObserveKey(Key.A), inputManager.ObserveKey(Key.Z),
                inputManager.ObserveKey(Key.W), inputManager.ObserveKey(Key.S), inputManager.ObserveKey(Key.X));
        }

        public void Tick(TimeSpan elapsed)
        {
            Clutch.Tick(elapsed);
            Brake.Tick(elapsed);
            Throttle.Tick(elapsed);
            Steering.Tick(elapsed);
        }
    }
}
