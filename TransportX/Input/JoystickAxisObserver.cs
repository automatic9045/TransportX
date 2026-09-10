using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input
{
    public class JoystickAxisObserver : IJoystickAxisObserver
    {
        public Guid DeviceGuid { get; }
        public JoystickAxisType AxisType { get; }

        public bool IsConnected { get; internal set; } = false;
        public int Value { get; internal set; } = 0;

        internal event EventHandler? Disposing;

        public JoystickAxisObserver(Guid deviceGuid, JoystickAxisType axisType)
        {
            DeviceGuid = deviceGuid;
            AxisType = axisType;
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);
        }
    }
}
