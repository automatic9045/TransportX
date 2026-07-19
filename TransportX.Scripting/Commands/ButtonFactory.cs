using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

using TransportX.Diagnostics;
using TransportX.Input;

using TransportX.Scripting.Input;

namespace TransportX.Scripting.Commands
{
    public class ButtonFactory
    {
        public Input Parent { get; }
        public object Context { get; }

        public string Key { get; }
        public KeyObserver? Observer { get; private set; } = null;

        public ScriptButton.KeyAction OnPressedAction { get; private set; } = _ => { };
        public ScriptButton.KeyAction OnReleasedAction { get; private set; } = _ => { };

        public ScriptButton? BuiltButton { get; private set; } = null;

        internal ButtonFactory(Input parent, object context, string key)
        {
            Parent = parent;
            Context = context;

            Key = key;
        }

        public ButtonFactory Bind(Key key)
        {
            Observer?.Dispose();
            Observer = Parent.InputManager.ObserveKey(key);
            return this;
        }

        public ButtonFactory Bind(string keyCode)
        {
            return ParseKeyOrReport(keyCode, out Key key) ? Bind(key) : this;
        }

        private bool ParseKeyOrReport(string keyCode, [MaybeNullWhen(false)] out Key key)
        {
            if (Enum.TryParse(keyCode, out key))
            {
                return true;
            }
            else
            {
                ScriptError error = new(ErrorLevel.Error, $"キーコード '{keyCode}' は存在しません。");
                Parent.ErrorCollector.Report(error);
                return false;
            }
        }

        public ButtonFactory OnPressed(ScriptButton.KeyAction action)
        {
            OnPressedAction = action;
            return this;
        }

        public ButtonFactory OnReleased(ScriptButton.KeyAction action)
        {
            OnReleasedAction = action;
            return this;
        }

        public ButtonFactory ForwardToSignal(string boolSignalKey)
        {
            Parent.Signals.ForwardBool(boolSignalKey, () => BuiltButton is null ? false : BuiltButton.IsPressed);
            return this;
        }

        public ScriptButton Build()
        {
            BuiltButton = new ScriptButton(Key)
            {
                Observer = Observer,
                OnPressed = OnPressedAction,
                OnReleased = OnReleasedAction,
            };
            Parent.AddButton(BuiltButton);
            return BuiltButton;
        }
    }
}
