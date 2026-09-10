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

        public IInputContext SilkContext { get; }
        public IDirectInput8 DirectInput { get; }

        private readonly Dictionary<Guid, IDirectInputDevice8> JoystickDevicesKey = [];
        public IReadOnlyDictionary<Guid, IDirectInputDevice8> JoystickDevices => JoystickDevicesKey;

        private readonly Dictionary<Guid, JoystickState> JoystickStatesKey = [];
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
            foreach (IDirectInputDevice8 device in JoystickDevicesKey.Values)
            {
                device.Unacquire();
                device.Dispose();
            }

            JoystickDevicesKey.Clear();
            DirectInput.Dispose();
        }

        public void RefreshDevices()
        {
            IList<DeviceInstance> instances = DirectInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

            List<Guid> toRemove = [];
            foreach (Guid guid in JoystickDevicesKey.Keys)
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
                IDirectInputDevice8 device = JoystickDevicesKey[guid];
                device.Unacquire();
                device.Dispose();
                JoystickDevicesKey.Remove(guid);
                JoystickStatesKey.Remove(guid);
            }

            foreach (DeviceInstance instance in instances)
            {
                if (!JoystickDevicesKey.ContainsKey(instance.InstanceGuid))
                {
                    IDirectInputDevice8 device = DirectInput.CreateDevice(instance.InstanceGuid);
                    device.SetDataFormat<RawJoystickState>();
                    device.SetCooperativeLevel(Hwnd, CooperativeLevel.Background | CooperativeLevel.Exclusive);

                    device.Acquire();

                    JoystickDevicesKey.Add(instance.InstanceGuid, device);
                    JoystickStatesKey.Add(instance.InstanceGuid, new JoystickState());
                }
            }
        }

        public void Tick(TimeSpan elapsed)
        {
            foreach ((Guid guid, IDirectInputDevice8 device) in JoystickDevicesKey)
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
