using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

namespace TransportX.Input.Configuration
{
    public class AxisBinding
    {
        public IReadOnlyDictionary<string, KeyboardAxisBinding> KeyboardPlus { get; init; } = new Dictionary<string, KeyboardAxisBinding>();
        public IReadOnlyDictionary<string, KeyboardAxisBinding> KeyboardMinus { get; init; } = new Dictionary<string, KeyboardAxisBinding>();
        public IReadOnlyDictionary<string, KeyboardAxisBinding> KeyboardReset { get; init; } = new Dictionary<string, KeyboardAxisBinding>();

        public IReadOnlyList<JoystickAxisBinding> Joysticks { get; init; } = [];
        public IReadOnlyList<JoystickPovAxisBinding> JoystickPovs { get; init; } = [];

        public AxisBinding()
        {
        }
    }
}
