using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input
{
    public class JoystickPovObserver : IDisposable
    {
        public const int AxisMin = -AxisMax;
        public const int AxisNeutral = 0;
        public const int AxisMax = 100000;


        public Guid DeviceGuid { get; }
        public int PovIndex { get; }

        public bool IsConnected { get; internal set; } = false;

        public bool IsUp { get; private set; } = false;
        public bool IsRight { get; private set; } = false;
        public bool IsDown { get; private set; } = false;
        public bool IsLeft { get; private set; } = false;

        public int X { get; private set; } = 0;
        public int Y { get; private set; } = 0;

        internal event EventHandler? Disposing;

        public JoystickPovObserver(Guid deviceGuid, int povIndex)
        {
            DeviceGuid = deviceGuid;
            PovIndex = povIndex;
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);
        }

        internal void Update(int povValue)
        {
            if (povValue == -1)
            {
                X = 0;
                Y = 0;
                IsUp = false;
                IsRight = false;
                IsDown = false;
                IsLeft = false;
            }
            else
            {
                float rad = povValue * float.Pi / 18000;
                X = (int)(float.Sin(rad) * 100000);
                Y = (int)(-float.Cos(rad) * 100000);

                IsUp = Y < -50000;
                IsDown = 50000 < Y;
                IsRight = 50000 < X;
                IsLeft = X < -50000;
            }
        }

        public bool IsPressed(JoystickPovDirection direction)
        {
            return direction switch
            {
                JoystickPovDirection.Up => IsUp,
                JoystickPovDirection.Right => IsRight,
                JoystickPovDirection.Down => IsDown,
                JoystickPovDirection.Left => IsLeft,
                _ => false,
            };
        }

        public int GetValue(JoystickPovAxisType axis)
        {
            return axis switch
            {
                JoystickPovAxisType.X => X,
                JoystickPovAxisType.Y => Y,
                _ => 0,
            };
        }

        public IJoystickButtonObserver AsButton(JoystickPovDirection direction) => new ButtonObserver(this, direction);
        public IJoystickAxisObserver AsAxis(JoystickPovAxisType axis) => new AxisObserver(this, axis);


        private class ButtonObserver : IJoystickButtonObserver
        {
            private readonly JoystickPovObserver Source;
            private readonly JoystickPovDirection Direction;

            public bool IsPressed => Source.IsPressed(Direction);

            public ButtonObserver(JoystickPovObserver source, JoystickPovDirection direction)
            {
                Source = source;
                Direction = direction;
            }

            public void Dispose()
            {
                Source.Dispose();
            }
        }

        private class AxisObserver : IJoystickAxisObserver
        {
            private readonly JoystickPovObserver Source;
            private readonly JoystickPovAxisType Axis;

            public bool IsConnected => Source.IsConnected;
            public int Value => Source.GetValue(Axis);

            public AxisObserver(JoystickPovObserver source, JoystickPovAxisType axis)
            {
                Source = source;
                Axis = axis;
            }

            public void Dispose()
            {
                Source.Dispose();
            }
        }
    }
}
