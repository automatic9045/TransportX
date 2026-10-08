using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Input;
using TransportX.Input.Configuration;

using TransportX.Scripting.Collections;
using TransportX.Scripting.Input;

namespace TransportX.Scripting.Commands
{
    public abstract class InputBase<T> where T : InputBase<T>
    {
        internal IInputClient InputClient { get; }
        internal Signals Signals { get; }
        internal IErrorCollector ErrorCollector { get; }

        public abstract InputProfile Profile { get; set; }

        private readonly ScriptKeyedList<string, ButtonCommand> ButtonsKey;
        public IReadOnlyScriptKeyedList<string, ButtonCommand> Buttons => ButtonsKey;

        private readonly ScriptKeyedList<string, AxisCommand> AxesKey;
        public IReadOnlyScriptKeyedList<string, AxisCommand> Axes => AxesKey;

        internal InputBase(IInputClient inputClient, Signals signals, IErrorCollector errorCollector)
        {
            InputClient = inputClient;
            Signals = signals;
            ErrorCollector = errorCollector;

            ButtonsKey = new ScriptKeyedList<string, ButtonCommand>(button => button.Key, ErrorCollector, "ボタン", ButtonCommand.Empty);
            AxesKey = new ScriptKeyedList<string, AxisCommand>(axis => axis.Key, ErrorCollector, "軸", AxisCommand.Empty);
        }

        internal void Dispose()
        {
            foreach (ButtonCommand buttonCommand in Buttons)
            {
                buttonCommand.Button.Dispose();
            }

            foreach (AxisCommand axisCommand in Axes)
            {
                axisCommand.Axis.Dispose();
            }
        }

        public InputProfile SetProfile(string key)
        {
            if (!InputClient.Profiles.TryGetValue(key, out InputProfile? profile))
            {
                profile = InputProfile.Empty(key);
            }

            Profile = profile;
            return profile;
        }

        public void AddButton(IButton button)
        {
            ButtonCommand buttonCommand = new(button);
            ButtonsKey.Add(buttonCommand);
        }

        public abstract ButtonFactory<T> AddButton(string key);

        public void AddAxis(IAxis axis)
        {
            AxisCommand axisCommand = new(axis);
            AxesKey.Add(axisCommand);
        }

        public abstract AxisFactory<T> AddAxis(string key, double min, double neutral, double max);

        internal void Tick(TimeSpan elapsed)
        {
            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].Button.Tick(elapsed);
            }

            for (int i = 0; i < Axes.Count; i++)
            {
                Axes[i].Axis.Tick(elapsed);
            }
        }
    }
}
