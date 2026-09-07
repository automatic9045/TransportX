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

        private readonly ScriptKeyedList<string, IButton> ButtonsKey;
        public IReadOnlyScriptKeyedList<string, IButton> Buttons => ButtonsKey;

        private readonly ScriptKeyedList<string, IAxis> AxesKey;
        public IReadOnlyScriptKeyedList<string, IAxis> Axes => AxesKey;

        internal InputBase(IInputClient inputClient, Signals signals, IErrorCollector errorCollector)
        {
            InputClient = inputClient;
            Signals = signals;
            ErrorCollector = errorCollector;

            ButtonsKey = new ScriptKeyedList<string, IButton>(button => button.Key, ErrorCollector, "ボタン", ScriptButton.Empty);
            AxesKey = new ScriptKeyedList<string, IAxis>(axis => axis.Key, ErrorCollector, "軸", ScriptAxis.Empty);
        }

        internal void Dispose()
        {
            foreach (IButton button in Buttons)
            {
                button.Dispose();
            }

            foreach (IAxis axis in Axes)
            {
                axis.Dispose();
            }
        }

        public InputProfile SetProfile(string key)
        {
            if (!InputClient.Profiles.TryGetValue(key, out InputProfile? profile))
            {
                ScriptError error = new(ErrorLevel.Error, $"入力プロファイル '{key}' は存在しません。");
                ErrorCollector.Report(error);
                return InputProfile.Empty(key);
            }

            Profile = profile;
            return profile;
        }

        public void AddButton(IButton button)
        {
            ButtonsKey.Add(button);
        }

        public abstract ButtonFactory<T> AddButton(string key);

        public void AddAxis(IAxis axis)
        {
            AxesKey.Add(axis);
        }

        public abstract AxisFactory<T> AddAxis(string key, double min, double neutral, double max);

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
