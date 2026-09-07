using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input.Configuration
{
    public class JoystickButtonBinding
    {
        public Guid DeviceGuid { get; }
        public int ButtonIndex { get; }

        public JoystickButtonBinding(Guid deviceGuid, int buttonIndex)
        {
            DeviceGuid = deviceGuid;
            ButtonIndex = buttonIndex;
        }
    }
}
