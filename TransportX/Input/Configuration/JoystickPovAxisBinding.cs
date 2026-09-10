using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input.Configuration
{
    public class JoystickPovAxisBinding
    {
        public Guid DeviceGuid { get; }
        public int PovIndex { get; }
        public JoystickPovAxisType AxisType { get; }

        public int RawMin { get; init; } = 0;
        public int RawNeutral { get; init; } = 0;
        public int RawMax { get; init; } = 0;
        public bool IsInverted { get; init; } = false;

        public JoystickPovAxisBinding(Guid deviceGuid, int povIndex, JoystickPovAxisType axisType)
        {
            DeviceGuid = deviceGuid;
            PovIndex = povIndex;
            AxisType = axisType;
        }
    }
}
