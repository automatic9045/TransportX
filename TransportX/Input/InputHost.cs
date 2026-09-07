using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpGen.Runtime;
using Silk.NET.Input;
using Vortice.DirectInput;

namespace TransportX.Input
{
    public class InputHost : IInputHost
    {
        private readonly nint Hwnd;

        private readonly Dictionary<Guid, IDirectInputDevice8> JoystickDevices = [];
        private readonly Dictionary<Guid, JoystickState> JoystickStatesKey = [];

        public IInputContext SilkContext { get; }
        public IDirectInput8 DirectInput { get; }

        public IReadOnlyDictionary<Guid, JoystickState> JoystickStates => JoystickStatesKey;

        public InputHost(IInputContext silkContext, nint hwnd)
        {
            SilkContext = silkContext;
            Hwnd = hwnd;
            DirectInput = DInput.DirectInput8Create();

            RefreshDevices();
        }

        public void Dispose()
        {
            foreach (IDirectInputDevice8 device in JoystickDevices.Values)
            {
                device.Unacquire();
                device.Dispose();
            }

            JoystickDevices.Clear();
            DirectInput.Dispose();
        }

        public void RefreshDevices()
        {
            IList<DeviceInstance> instances = DirectInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

            List<Guid> toRemove = [];
            foreach (Guid guid in JoystickDevices.Keys)
            {
                bool found = false;
                foreach (DeviceInstance instance in instances)
                {
                    if (instance.InstanceGuid == guid)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    toRemove.Add(guid);
                }
            }

            foreach (Guid guid in toRemove)
            {
                IDirectInputDevice8 device = JoystickDevices[guid];
                device.Unacquire();
                device.Dispose();
                JoystickDevices.Remove(guid);
                JoystickStatesKey.Remove(guid);
            }

            foreach (DeviceInstance instance in instances)
            {
                if (!JoystickDevices.ContainsKey(instance.InstanceGuid))
                {
                    IDirectInputDevice8 device = DirectInput.CreateDevice(instance.InstanceGuid);
                    device.SetDataFormat<RawJoystickState>();
                    device.SetCooperativeLevel(Hwnd, CooperativeLevel.Background | CooperativeLevel.Exclusive);

                    device.Acquire();

                    JoystickDevices.Add(instance.InstanceGuid, device);
                    JoystickStatesKey.Add(instance.InstanceGuid, new JoystickState());
                }
            }
        }

        public void Tick(TimeSpan elapsed)
        {
            foreach ((Guid guid, IDirectInputDevice8 device) in JoystickDevices)
            {
                Result result = device.Poll();
                if (result.Failure)
                {
                    device.Acquire();
                    continue;
                }

                JoystickState state = device.GetCurrentJoystickState();
                JoystickStatesKey[guid] = state;
            }
        }
    }
}
