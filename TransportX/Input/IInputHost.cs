using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Vortice.DirectInput;

namespace TransportX.Input
{
    public interface IInputHost : IDisposable
    {
        IInputContext SilkContext { get; }
        IDirectInput8 DirectInput { get; }

        IReadOnlyDictionary<Guid, JoystickState> JoystickStates { get; }

        void RefreshDevices();
        void Tick(TimeSpan elapsed);
    }
}
