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
        public string Key { get; }

        public required KeyObserver? Observer { get; init; }
        public required KeyAction OnPressed { get; init; }
        public required KeyAction OnReleased { get; init; }

        public bool IsPressed { get; private set; }

        public event ButtonEventHandler? Pressed;
        public event ButtonEventHandler? Released;

        internal ScriptButton(string key)
        {
            Key = key;
        }

        internal static ScriptButton Empty(string key)
        {
            return new ScriptButton(key)
            {
                Observer = null,
                OnPressed = _ => { },
                OnReleased = _ => { },
            };
        }

        public void Dispose()
        {
            Observer?.Dispose();
        }

        public void Tick(TimeSpan elapsed)
        {
            if (Observer is null) return;

            bool latestIsPressed = Observer.IsPressed;
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
