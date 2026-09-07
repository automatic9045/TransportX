using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input.Configuration
{
    public class JoystickAxisBinding
    {
        public Guid DeviceGuid { get; }
        public JoystickAxisType AxisType { get; }

        public int RawMin { get; init; } = 0;
        public int RawNeutral { get; init; } = 0;
        public int RawMax { get; init; } = 0;
        public bool IsInverted { get; init; } = false;

        public JoystickAxisBinding(Guid deviceGuid, JoystickAxisType axisType)
        {
            DeviceGuid = deviceGuid;
            AxisType = axisType;
        }
    }
}
