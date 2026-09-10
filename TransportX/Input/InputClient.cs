using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using SilkKey = Silk.NET.Input.Key;
using Vortice.DirectInput;

using TransportX.Input.Configuration;

namespace TransportX.Input
{
    public class InputClient : IInputClient
    {
        private readonly IInputHost InputHost;

        private readonly ConcurrentDictionary<SilkKey, List<KeyObserver>> Keys = new();
        private readonly List<JoystickButtonObserver> JoystickButtons = [];
        private readonly List<JoystickAxisObserver> JoystickAxes = [];
        private readonly List<JoystickPovObserver> JoystickPovs = [];

        private Vector2 OldMousePosition = Vector2.NaN;

        public IReadOnlyDictionary<string, InputProfile> Profiles { get; }

        public event MouseScrollEventHandler? MouseScroll;
        public event MouseMoveEventHandler? MouseMove;

        public InputClient(IInputHost inputHost, IReadOnlyDictionary<string, InputProfile> profiles)
        {
            InputHost = inputHost;
            Profiles = profiles;

            InputHost.SilkContext.ConnectionChanged += OnSilkConnectionChanged;

            for (int i = 0; i < InputHost.SilkContext.Keyboards.Count; i++) RegisterKeyboard(InputHost.SilkContext.Keyboards[i]);
            for (int i = 0; i < InputHost.SilkContext.Mice.Count; i++) RegisterMouse(InputHost.SilkContext.Mice[i]);
        }

        private void OnSilkConnectionChanged(IInputDevice device, bool isConnected)
        {
            switch (device)
            {
                case IKeyboard keyboard:
                    if (isConnected) RegisterKeyboard(keyboard); else UnregisterKeyboard(keyboard);
                    break;

                case IMouse mouse:
                    if (isConnected) RegisterMouse(mouse); else UnregisterMouse(mouse);
                    break;
            }
        }

        private void RegisterKeyboard(IKeyboard keyboard)
        {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
        }

        private void UnregisterKeyboard(IKeyboard keyboard)
        {
            keyboard.KeyDown -= OnKeyDown;
            keyboard.KeyUp -= OnKeyUp;
        }

        private void RegisterMouse(IMouse mouse)
        {
            mouse.Scroll += OnMouseScroll;
            mouse.MouseMove += OnMouseMove;
        }

        private void UnregisterMouse(IMouse mouse)
        {
            mouse.Scroll -= OnMouseScroll;
            mouse.MouseMove -= OnMouseMove;
        }

        private void OnKeyDown(IKeyboard keyboard, SilkKey key, int keyCode)
        {
            if (Keys.TryGetValue(key, out List<KeyObserver>? list))
            {
                foreach (KeyObserver observer in list)
                {
                    observer.Press(keyboard);
                }
            }
        }

        private void OnKeyUp(IKeyboard keyboard, SilkKey key, int keyCode)
        {
            if (Keys.TryGetValue(key, out List<KeyObserver>? list))
            {
                foreach (KeyObserver observer in list)
                {
                    observer.Release(keyboard);
                }
            }
        }

        private void OnMouseScroll(IMouse mouse, ScrollWheel delta)
        {
            MouseScroll?.Invoke(mouse, delta);
        }

        private void OnMouseMove(IMouse mouse, Vector2 position)
        {
            MouseMove?.Invoke(mouse, Vector2.IsNaN(OldMousePosition) != Vector2.Zero ? Vector2.Zero : position - OldMousePosition);
            OldMousePosition = position;
        }

        public KeyObserver ObserveKey(SilkKey key)
        {
            KeyObserver observer = new(key);
            observer.Disposing += (sender, e) => Keys[key].Remove(observer);

            List<KeyObserver> list = Keys.GetOrAdd(key, []);
            list.Add(observer);
            return observer;
        }

        public JoystickButtonObserver ObserveJoystickButton(Guid deviceGuid, int buttonIndex)
        {
            JoystickButtonObserver observer = new(deviceGuid, buttonIndex);
            observer.Disposing += (sender, e) => JoystickButtons.Remove(observer);

            JoystickButtons.Add(observer);
            return observer;
        }

        public JoystickAxisObserver ObserveJoystickAxis(Guid deviceGuid, JoystickAxisType axisType)
        {
            JoystickAxisObserver observer = new(deviceGuid, axisType);
            observer.Disposing += (sender, e) => JoystickAxes.Remove(observer);

            JoystickAxes.Add(observer);
            return observer;
        }

        public JoystickPovObserver ObserveJoystickPov(Guid deviceGuid, int povIndex)
        {
            JoystickPovObserver observer = new(deviceGuid, povIndex);
            observer.Disposing += (sender, e) => JoystickPovs.Remove(observer);

            JoystickPovs.Add(observer);
            return observer;
        }

        public void Tick(TimeSpan elapsed)
        {
            foreach (JoystickButtonObserver observer in JoystickButtons)
            {
                if (!InputHost.JoystickStates.TryGetValue(observer.DeviceGuid, out JoystickState? state)) continue;

                bool isPressed = observer.ButtonIndex < state.Buttons.Length && state.Buttons[observer.ButtonIndex];
                observer.Update(isPressed);
            }

            foreach (JoystickAxisObserver observer in JoystickAxes)
            {
                if (!InputHost.JoystickStates.TryGetValue(observer.DeviceGuid, out JoystickState? state))
                {
                    observer.IsConnected = false;
                    continue;
                }

                observer.Value = observer.AxisType switch
                {
                    JoystickAxisType.X => state.X,
                    JoystickAxisType.Y => state.Y,
                    JoystickAxisType.Z => state.Z,
                    JoystickAxisType.RotationX => state.RotationX,
                    JoystickAxisType.RotationY => state.RotationY,
                    JoystickAxisType.RotationZ => state.RotationZ,
                    JoystickAxisType.Slider0 => 0 < state.Sliders.Length ? state.Sliders[0] : 0,
                    JoystickAxisType.Slider1 => 1 < state.Sliders.Length ? state.Sliders[1] : 0,
                    _ => 0,
                };
                observer.IsConnected = true;
            }

            foreach (JoystickPovObserver observer in JoystickPovs)
            {
                if (!InputHost.JoystickStates.TryGetValue(observer.DeviceGuid, out JoystickState? state))
                {
                    observer.IsConnected = false;
                    continue;
                }

                int value = observer.PovIndex < state.PointOfViewControllers.Length ? state.PointOfViewControllers[observer.PovIndex] : 0;
                observer.Update(value);
                observer.IsConnected = true;
            }
        }
    }
}
