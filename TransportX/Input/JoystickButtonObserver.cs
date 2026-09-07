using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input
{
    public class JoystickButtonObserver : IDisposable
    {
        public Guid DeviceGuid { get; }
        public int ButtonIndex { get; }

        public bool IsPressed { get; private set; }

        internal event EventHandler? Disposing;

        public event Action<JoystickButtonObserver>? Pressed;
        public event Action<JoystickButtonObserver>? Released;

        public JoystickButtonObserver(Guid deviceGuid, int buttonIndex)
        {
            DeviceGuid = deviceGuid;
            ButtonIndex = buttonIndex;
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);
        }

        internal void Update(bool isPressed)
        {
            if (IsPressed != isPressed)
            {
                IsPressed = isPressed;
                if (IsPressed)
                {
                    Pressed?.Invoke(this);
                }
                else
                {
                    Released?.Invoke(this);
                }
            }
        }
    }
}
