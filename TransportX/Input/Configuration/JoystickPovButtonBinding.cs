using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input.Configuration
{
    public class JoystickPovButtonBinding
    {
        public Guid DeviceGuid { get; }
        public int PovIndex { get; }
        public JoystickPovDirection Direction { get; }

        public JoystickPovButtonBinding(Guid deviceGuid, int povIndex, JoystickPovDirection direction)
        {
            DeviceGuid = deviceGuid;
            PovIndex = povIndex;
            Direction = direction;
        }
    }
}
