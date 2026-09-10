using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

namespace TransportX.Input.Configuration
{
    public class ButtonBinding
    {
        public IReadOnlyList<Key> Keys { get; init; } = [];
        public IReadOnlyList<JoystickButtonBinding> Joysticks { get; init; } = [];
        public IReadOnlyList<JoystickPovButtonBinding> JoystickPovs { get; init; } = [];

        public ButtonBinding()
        {
        }
    }
}
