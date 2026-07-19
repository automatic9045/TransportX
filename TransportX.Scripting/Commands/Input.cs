using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Input;

using TransportX.Scripting.Collections;
using TransportX.Scripting.Input;

namespace TransportX.Scripting.Commands
{
    public class Input
    {
        private readonly object Context;

        internal Signals Signals { get; }
        internal InputManager InputManager { get; }
        internal IErrorCollector ErrorCollector { get; }

        private readonly ScriptKeyedList<string, IButton> ButtonsKey;
        public IReadOnlyScriptKeyedList<string, IButton> Buttons => ButtonsKey;

        private readonly ScriptKeyedList<string, IAxis> AxesKey;
        public IReadOnlyScriptKeyedList<string, IAxis> Axes => AxesKey;

        internal Input(Signals signals, InputManager inputManager, IErrorCollector errorCollector, object context)
        {
            Signals = signals;
            InputManager = inputManager;
            ErrorCollector = errorCollector;
            Context = context;

            ButtonsKey = new ScriptKeyedList<string, IButton>(button => button.Key, ErrorCollector, "ボタン", ScriptButton.Empty);
            AxesKey = new ScriptKeyedList<string, IAxis>(axis => axis.Key, ErrorCollector, "軸", ScriptAxis.Empty);
        }

        internal void Dispose()
        {
            foreach (ScriptButton button in Buttons)
            {
                button.Dispose();
            }

            foreach (ScriptAxis axis in Axes)
            {
                axis.Dispose();
            }
        }

        public void AddButton(IButton button)
        {
            ButtonsKey.Add(button);
        }

        public ButtonFactory AddButton(string key)
        {
            ButtonFactory buttonFactory = new(this, Context, key);
            return buttonFactory;
        }

        public void AddAxis(IAxis axis)
        {
            AxesKey.Add(axis);
        }

        public AxisFactory AddAxis(string key, double min, double neutral, double max)
        {
            AxisFactory axisFactory = new(this, Context, key, (float)min, (float)neutral, (float)max);
            return axisFactory;
        }

        internal void Tick(TimeSpan elapsed)
        {
            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].Tick(elapsed);
            }

            for (int i = 0; i < Axes.Count; i++)
            {
                Axes[i].Tick(elapsed);
            }
        }
    }
}
