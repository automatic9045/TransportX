using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

using TransportX.Input.Configuration;

namespace TransportX.Input
{
    public interface IInputClient
    {
        IReadOnlyDictionary<string, InputProfile> Profiles { get; }

        event MouseScrollEventHandler? MouseScroll;
        event MouseMoveEventHandler? MouseMove;

        KeyObserver ObserveKey(Key key);
        JoystickButtonObserver ObserveJoystickButton(Guid deviceGuid, int buttonIndex);
        JoystickAxisObserver ObserveJoystickAxis(Guid deviceGuid, JoystickAxisType axisType);

        void Tick(TimeSpan elapsed);
    }
}
