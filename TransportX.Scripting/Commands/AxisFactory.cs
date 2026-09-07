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
    public class AxisFactory<TParent> where TParent : InputBase<TParent>
    {
        public TParent Parent { get; }

        public string Key { get; }
        public float Min { get; }
        public float Neutral { get; }
        public float Max { get; }
        public float InitialValue { get; private set; }

        private readonly List<ScriptAxis.KeyBinding> PlusBindingsKey = [];
        public IReadOnlyList<ScriptAxis.KeyBinding> PlusBindings => PlusBindingsKey;

        private readonly List<ScriptAxis.KeyBinding> MinusBindingsKey = [];
        public IReadOnlyList<ScriptAxis.KeyBinding> MinusBindings => MinusBindingsKey;

        private readonly List<ScriptAxis.KeyBinding> ResetBindingsKey = [];
        public IReadOnlyList<ScriptAxis.KeyBinding> ResetBindings => ResetBindingsKey;

        private readonly List<ScriptAxis.JoystickAxisBinding> JoystickBindingsKey = [];
        public IReadOnlyList<ScriptAxis.JoystickAxisBinding> JoystickBindings => JoystickBindingsKey;

        public float AutoReleaseSpeed { get; private set; } = 0;
        public ScriptAxis.TickFunc OnTickFunc { get; private set; }

        public ScriptAxis? BuiltAxis { get; private set; } = null;

        internal AxisFactory(TParent parent, object context, string key, float min, float neutral, float max)
        {
            Parent = parent;

            Key = key;
            Min = min;
            Neutral = neutral;
            Max = max;
            InitialValue = neutral;

            OnTickFunc = ScriptAxis.TickDefault;
        }

        public AxisFactory<TParent> SetInitialValue(double value)
        {
            InitialValue = (float)value;
            return this;
        }

        public AxisFactory<TParent> BindPlus(string key, Key defaultBinding, ScriptAxis.SpeedFunc speedFunc)
        {
            return Bind(key, defaultBinding, speedFunc, binding => binding.KeyboardPlus, PlusBindingsKey);
        }

        public AxisFactory<TParent> BindPlus(string key, Key defaultBinding, double engageSpeed, double releaseSpeed, double maxValue)
        {
            float floatEngageSpeed = (float)engageSpeed;
            float floatReleaseSpeed = (float)releaseSpeed;
            return BindPlus(key, defaultBinding, (instance, observer) =>
            {
                return maxValue < instance.Value ? 0
                    : instance.Neutral < instance.Value ? floatEngageSpeed
                    : floatReleaseSpeed;
            });
        }

        public AxisFactory<TParent> BindPlus(string key, Key defaultBinding, double engageSpeed, double releaseSpeed)
            => BindPlus(key, defaultBinding, engageSpeed, releaseSpeed, Max);
        public AxisFactory<TParent> BindPlus(string key, Key defaultBinding, double speed)
            => BindPlus(key, defaultBinding, speed, speed);
        public AxisFactory<TParent> BindPlus(string key, string defaultBindingCode, ScriptAxis.SpeedFunc speedFunc)
            => ParseKeyOrReport(defaultBindingCode, out Key defaultBinding) ? BindPlus(key, defaultBinding, speedFunc) : this;
        public AxisFactory<TParent> BindPlus(string key, string defaultBindingCode, double engageSpeed, double releaseSpeed, double maxValue)
            => ParseKeyOrReport(defaultBindingCode, out Key defaultBinding) ? BindPlus(key, defaultBinding, engageSpeed, releaseSpeed, maxValue) : this;
        public AxisFactory<TParent> BindPlus(string key, string defaultBindingCode, double engageSpeed, double releaseSpeed)
            => BindPlus(key, defaultBindingCode, engageSpeed, releaseSpeed, Max);
        public AxisFactory<TParent> BindPlus(string key, string defaultBindingCode, double speed)
            => BindPlus(key, defaultBindingCode, speed, speed);

        public AxisFactory<TParent> BindPlus(Key defaultBinding, double engageSpeed, double releaseSpeed)
            => BindPlus(string.Empty, defaultBinding, engageSpeed, releaseSpeed);
        public AxisFactory<TParent> BindPlus(Key defaultBinding, double speed)
            => BindPlus(string.Empty, defaultBinding, speed);
        public AxisFactory<TParent> BindPlus(string defaultBindingCode, ScriptAxis.SpeedFunc speedFunc)
            => BindPlus(string.Empty, defaultBindingCode, speedFunc);
        public AxisFactory<TParent> BindPlus(string defaultBindingCode, double engageSpeed, double releaseSpeed, double maxValue)
            => BindPlus(string.Empty, defaultBindingCode, engageSpeed, releaseSpeed, maxValue);
        public AxisFactory<TParent> BindPlus(string defaultBindingCode, double engageSpeed, double releaseSpeed)
            => BindPlus(string.Empty, defaultBindingCode, engageSpeed, releaseSpeed);
        public AxisFactory<TParent> BindPlus(string defaultBindingCode, double speed)
            => BindPlus(string.Empty, defaultBindingCode, speed);

        public AxisFactory<TParent> BindMinus(string key, Key defaultBinding, ScriptAxis.SpeedFunc speedFunc)
        {
            return Bind(key, defaultBinding, speedFunc, binding => binding.KeyboardMinus, MinusBindingsKey);
        }

        public AxisFactory<TParent> BindMinus(string key, Key defaultBinding, double engageSpeed, double releaseSpeed, double minValue)
        {
            float floatEngageSpeed = (float)engageSpeed;
            float floatReleaseSpeed = (float)releaseSpeed;
            return BindMinus(key, defaultBinding, (instance, observer) =>
            {
                return instance.Value < minValue ? 0
                    : instance.Neutral < instance.Value ? floatEngageSpeed
                    : floatReleaseSpeed;
            });
        }

        public AxisFactory<TParent> BindMinus(string key, Key defaultBinding, double engageSpeed, double releaseSpeed)
            => BindMinus(key, defaultBinding, engageSpeed, releaseSpeed, Max);
        public AxisFactory<TParent> BindMinus(string key, Key defaultBinding, double speed)
            => BindMinus(key, defaultBinding, speed);
        public AxisFactory<TParent> BindMinus(string key, string defaultBindingCode, ScriptAxis.SpeedFunc speedFunc)
            => ParseKeyOrReport(defaultBindingCode, out Key defaultBinding) ? BindMinus(key, defaultBinding, speedFunc) : this;
        public AxisFactory<TParent> BindMinus(string key, string defaultBindingCode, double engageSpeed, double releaseSpeed, double minValue)
            => ParseKeyOrReport(defaultBindingCode, out Key defaultBinding) ? BindMinus(key, defaultBinding, engageSpeed, releaseSpeed, minValue) : this;
        public AxisFactory<TParent> BindMinus(string key, string defaultBindingCode, double engageSpeed, double releaseSpeed)
            => BindMinus(key, defaultBindingCode, engageSpeed, releaseSpeed, Max);
        public AxisFactory<TParent> BindMinus(string key, string defaultBindingCode, double speed)
            => BindMinus(key, defaultBindingCode, speed, speed);

        public AxisFactory<TParent> BindMinus(Key defaultBinding, double engageSpeed, double releaseSpeed)
            => BindMinus(string.Empty, defaultBinding, engageSpeed, releaseSpeed);
        public AxisFactory<TParent> BindMinus(Key defaultBinding, double speed)
            => BindMinus(string.Empty, defaultBinding, speed);
        public AxisFactory<TParent> BindMinus(string defaultBindingCode, ScriptAxis.SpeedFunc speedFunc)
            => BindMinus(string.Empty, defaultBindingCode, speedFunc);
        public AxisFactory<TParent> BindMinus(string defaultBindingCode, double engageSpeed, double releaseSpeed, double maxValue)
            => BindMinus(string.Empty, defaultBindingCode, engageSpeed, releaseSpeed, maxValue);
        public AxisFactory<TParent> BindMinus(string defaultBindingCode, double engageSpeed, double releaseSpeed)
            => BindMinus(string.Empty, defaultBindingCode, engageSpeed, releaseSpeed);
        public AxisFactory<TParent> BindMinus(string defaultBindingCode, double speed)
            => BindMinus(string.Empty, defaultBindingCode, speed);

        public AxisFactory<TParent> BindReset(string key, Key defaultBinding, ScriptAxis.SpeedFunc speedFunc)
        {
            return Bind(key, defaultBinding, speedFunc, binding => binding.KeyboardReset, ResetBindingsKey);
        }

        public AxisFactory<TParent> BindReset(string key, Key defaultBinding, double speed)
        {
            float floatSpeed = (float)speed;
            return BindReset(key, defaultBinding, (_, _) => floatSpeed);
        }

        public AxisFactory<TParent> BindReset(string key, string defaultBindingCode, ScriptAxis.SpeedFunc speedFunc)
            => ParseKeyOrReport(defaultBindingCode, out Key defaultBinding) ? BindReset(key, defaultBinding, speedFunc) : this;
        public AxisFactory<TParent> BindReset(string key, string defaultBindingCode, double speed)
            => ParseKeyOrReport(defaultBindingCode, out Key defaultBinding) ? BindReset(key, defaultBinding, speed) : this;

        public AxisFactory<TParent> BindReset(string defaultBindingCode, ScriptAxis.SpeedFunc speedFunc)
            => BindReset(string.Empty, defaultBindingCode, speedFunc);
        public AxisFactory<TParent> BindReset(string defaultBindingCode, double speed)
            => BindReset(string.Empty, defaultBindingCode, speed);

        private AxisFactory<TParent> Bind(string key, Key defaultBinding, ScriptAxis.SpeedFunc speedFunc,
            Func<AxisBinding, IReadOnlyDictionary<string, KeyboardAxisBinding>> dictionarySelector, List<ScriptAxis.KeyBinding> bindings)
        {
            bool hasBoundFromProfile = false;
            if (Parent.Profile.AxisBindings.TryGetValue(Key, out AxisBinding? axisBinding))
            {
                if (dictionarySelector(axisBinding).TryGetValue(key, out KeyboardAxisBinding? keyboardBinding))
                {
                    foreach (Key silkKey in keyboardBinding.Keys)
                    {
                        bindings.Add(new ScriptAxis.KeyBinding(Parent.InputClient.ObserveKey(silkKey), speedFunc));
                        hasBoundFromProfile = true;
                    }
                }
            }

            if (!hasBoundFromProfile)
            {
                bindings.Add(new ScriptAxis.KeyBinding(Parent.InputClient.ObserveKey(defaultBinding), speedFunc));
            }

            return this;
        }

        public bool ParseKeyOrReport(string keyCode, [MaybeNullWhen(false)] out Key key)
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

        public AxisFactory<TParent> AutoRelease(double speed)
        {
            AutoReleaseSpeed = (float)speed;
            return this;
        }

        public AxisFactory<TParent> OnTick(ScriptAxis.TickFunc func)
        {
            OnTickFunc = func;
            return this;
        }

        public AxisFactory<TParent> ForwardToSignal(string floatSignalKey)
        {
            Parent.Signals.ForwardFloat(floatSignalKey, () => BuiltAxis is null ? 0 : BuiltAxis.Value);
            return this;
        }

        public ScriptAxis Build()
        {
            if (Parent.Profile.AxisBindings.TryGetValue(Key, out AxisBinding? binding))
            {
                foreach (JoystickAxisBinding joystick in binding.Joysticks)
                {
                    if (!JoystickBindingsKey.Any(x => x.Observer.DeviceGuid == joystick.DeviceGuid && x.Observer.AxisType == joystick.AxisType))
                    {
                        JoystickAxisObserver observer = Parent.InputClient.ObserveJoystickAxis(joystick.DeviceGuid, joystick.AxisType);
                        JoystickBindingsKey.Add(new ScriptAxis.JoystickAxisBinding(observer, joystick.RawMin, joystick.RawNeutral, joystick.RawMax, joystick.IsInverted));
                    }
                }
            }

            BuiltAxis = new ScriptAxis(Key, Min, Neutral, Max, InitialValue)
            {
                PlusBindings = PlusBindings,
                MinusBindings = MinusBindings,
                ResetBindings = ResetBindings,
                JoystickBindings = JoystickBindings,
                AutoReleaseSpeed = AutoReleaseSpeed,
                OnTick = OnTickFunc,
            };
            Parent.AddAxis(BuiltAxis);
            return BuiltAxis;
        }
    }
}
