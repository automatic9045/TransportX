using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

using TransportX.Diagnostics;
using TransportX.Input;
using TransportX.Input.Configuration;

using TransportX.Scripting.Input;

namespace TransportX.Scripting.Commands
{
    public class ButtonFactory<TParent> where TParent : InputBase<TParent>
    {
        public TParent Parent { get; }

        public string Key { get; }

        private readonly List<KeyObserver> KeyboardObserversKey = [];
        public IReadOnlyList<KeyObserver> KeyboardObservers => KeyboardObserversKey;

        private readonly List<JoystickButtonObserver> JoystickObserversKey = [];
        public IReadOnlyList<JoystickButtonObserver> JoystickObservers => JoystickObserversKey;

        public ScriptButton.KeyAction OnPressedAction { get; private set; } = _ => { };
        public ScriptButton.KeyAction OnReleasedAction { get; private set; } = _ => { };

        public ScriptButton? BuiltButton { get; private set; } = null;

        internal ButtonFactory(TParent parent, string key)
        {
            Parent = parent;
            Key = key;
        }

        public ButtonFactory<TParent> Bind(Key defaultBinding)
        {
            bool hasBoundFromProfile = false;

            if (Parent.Profile.ButtonBindings.TryGetValue(Key, out ButtonBinding? binding))
            {
                foreach (Key silkKey in binding.Keys)
                {
                    KeyboardObserversKey.Add(Parent.InputClient.ObserveKey(silkKey));
                    hasBoundFromProfile = true;
                }

                foreach (JoystickButtonBinding joystick in binding.Joysticks)
                {
                    JoystickObserversKey.Add(Parent.InputClient.ObserveJoystickButton(joystick.DeviceGuid, joystick.ButtonIndex));
                }
            }

            if (!hasBoundFromProfile)
            {
                KeyboardObserversKey.Add(Parent.InputClient.ObserveKey(defaultBinding));
            }

            return this;
        }

        public ButtonFactory<TParent> Bind(string defaultBindingCode)
        {
            return ParseKeyOrReport(defaultBindingCode, out Key defaultBinding) ? Bind(defaultBinding) : this;
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

        public ButtonFactory<TParent> OnPressed(ScriptButton.KeyAction action)
        {
            OnPressedAction = action;
            return this;
        }

        public ButtonFactory<TParent> OnReleased(ScriptButton.KeyAction action)
        {
            OnReleasedAction = action;
            return this;
        }

        public ButtonFactory<TParent> ForwardToSignal(string boolSignalKey)
        {
            Parent.Signals.ForwardBool(boolSignalKey, () => BuiltButton is null ? false : BuiltButton.IsPressed);
            return this;
        }

        public ScriptButton Build()
        {
            BuiltButton = new ScriptButton(Key)
            {
                KeyboardObservers = KeyboardObservers,
                JoystickObservers = JoystickObservers,
                OnPressed = OnPressedAction,
                OnReleased = OnReleasedAction,
            };
            Parent.AddButton(BuiltButton);
            return BuiltButton;
        }
    }
}
