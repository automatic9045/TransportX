using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

namespace TransportX.Scripting.Input
{
    public class ScriptButton : IButton
    {
        internal static ScriptButton Empty(string key)
        {
            return new ScriptButton(key)
            {
                KeyboardObservers = [],
                JoystickObservers = [],
                OnPressed = _ => { },
                OnReleased = _ => { },
            };
        }


        public string Key { get; }

        public required IReadOnlyList<KeyObserver> KeyboardObservers { get; init; }
        public required IReadOnlyList<JoystickButtonObserver> JoystickObservers { get; init; }

        public required KeyAction OnPressed { get; init; }
        public required KeyAction OnReleased { get; init; }

        public bool IsPressed { get; private set; }

        public event ButtonEventHandler? Pressed;
        public event ButtonEventHandler? Released;

        internal ScriptButton(string key)
        {
            Key = key;
        }

        public void Dispose()
        {
            foreach (KeyObserver observer in KeyboardObservers) observer.Dispose();
            foreach (JoystickButtonObserver observer in JoystickObservers) observer.Dispose();
        }

        public void Tick(TimeSpan elapsed)
        {
            bool latestIsPressed = false;

            for (int i = 0; i < KeyboardObservers.Count; i++)
            {
                if (KeyboardObservers[i].IsPressed)
                {
                    latestIsPressed = true;
                    break;
                }
            }

            if (!latestIsPressed)
            {
                for (int i = 0; i < JoystickObservers.Count; i++)
                {
                    if (JoystickObservers[i].IsPressed)
                    {
                        latestIsPressed = true;
                        break;
                    }
                }
            }

            if (IsPressed != latestIsPressed)
            {
                IsPressed = latestIsPressed;
                if (IsPressed)
                {
                    OnPressed(this);
                    Pressed?.Invoke(this);
                }
                else
                {
                    OnReleased(this);
                    Released?.Invoke(this);
                }
            }
        }


        public delegate void KeyAction(ScriptButton instance);
    }
}
