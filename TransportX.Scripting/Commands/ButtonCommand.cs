using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

using TransportX.Scripting.Input;

namespace TransportX.Scripting.Commands
{
    public class ButtonCommand
    {
        public static ButtonCommand Empty(string key) => new(ScriptButton.Empty(key));


        public IButton Button { get; }

        public string Key => Button.Key;
        public bool IsPressed => Button.IsPressed;

        internal ButtonCommand(IButton button)
        {
            Button = button;
        }

        public void OnPressed(ButtonEventHandler action)
        {
            Button.Pressed += action;
        }

        public void OnReleased(ButtonEventHandler action)
        {
            Button.Pressed += action;
        }
    }
}
