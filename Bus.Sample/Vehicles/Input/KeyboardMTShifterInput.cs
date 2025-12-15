using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;

namespace Bus.Sample.Vehicles.Input
{
    internal class KeyboardMTShifterInput : IMTShifterInput
    {
        private readonly KeyObserver UpKey;
        private readonly KeyObserver DownKey;
        private readonly KeyObserver LeftKey;
        private readonly KeyObserver RightKey;

        public Vector2 Direction { get; private set; } = Vector2.Zero;

        public KeyboardMTShifterInput(KeyObserver upKey, KeyObserver downKey, KeyObserver leftKey, KeyObserver rightKey)
        {
            UpKey = upKey;
            DownKey = downKey;
            LeftKey = leftKey;
            RightKey = rightKey;
        }

        public void Tick(TimeSpan elapsed)
        {
            int x = (LeftKey.IsPressed ? -1 : 0) + (RightKey.IsPressed ? 1 : 0);
            int y = (DownKey.IsPressed ? -1 : 0) + (UpKey.IsPressed? 1 : 0);
            Direction = new Vector2(x, y);
        }
    }
}
