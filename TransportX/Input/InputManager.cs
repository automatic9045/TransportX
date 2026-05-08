using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

namespace TransportX.Input
{
    public class InputManager
    {
        private readonly IInputContext Input;
        private readonly ConcurrentDictionary<Key, List<KeyObserver>> KeyObservers = new();

        private Vector2 OldMousePosition = Vector2.NaN;

        public event MouseScrollEventHandler? MouseScroll;
        public event MouseMoveEventHandler? MouseMove;

        public InputManager(IInputContext input)
        {
            Input = input;
            Input.ConnectionChanged += OnConnectionChanged;

            for (int i = 0; i < Input.Keyboards.Count; i++) RegisterKeyboard(Input.Keyboards[i]);
            for (int i = 0; i < Input.Mice.Count; i++) RegisterMouse(Input.Mice[i]);
        }

        private void OnConnectionChanged(IInputDevice device, bool isConnected)
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

        private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if (KeyObservers.TryGetValue(key, out List<KeyObserver>? list))
            {
                foreach (KeyObserver observer in list)
                {
                    observer.Press(keyboard);
                }
            }
        }

        private void OnKeyUp(IKeyboard keyboard, Key key, int keyCode)
        {
            if (KeyObservers.TryGetValue(key, out List<KeyObserver>? list))
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

        public KeyObserver ObserveKey(Key key)
        {
            KeyObserver observer = new(key);
            observer.Disposing += (sender, e) => KeyObservers[key].Remove(observer);

            List<KeyObserver> list = KeyObservers.GetOrAdd(key, []);
            list.Add(observer);
            return observer;
        }
    }
}
