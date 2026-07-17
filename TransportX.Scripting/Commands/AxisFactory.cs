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
    public class AxisFactory
    {
        public Input Parent { get; }
        public object Context { get; }

        public string Key { get; }
        public float Min { get; }
        public float Neutral { get; }
        public float Max { get; }
        public float InitialValue { get; private set; }

        public ScriptAxis.KeyBinding Plus { get; private set; } = default;
        public ScriptAxis.KeyBinding Minus { get; private set; } = default;
        public ScriptAxis.KeyBinding Reset { get; private set; } = default;
        public float AutoReleaseSpeed { get; private set; } = 0;

        public ScriptAxis.TickFunc OnTickFunc { get; private set; }

        public ScriptAxis? BuiltAxis { get; private set; } = null;

        internal AxisFactory(Input parent, object context, string key, float min, float neutral, float max)
        {
            Parent = parent;
            Context = context;

            Key = key;
            Min = min;
            Neutral = neutral;
            Max = max;
            InitialValue = neutral;

            bool isResetting = false;
            OnTickFunc = OnTickDefault;


            float OnTickDefault(ScriptAxis instance, float dt)
            {
                int sign = float.Sign(instance.Value - instance.Neutral);
                float speed = 0;

                bool isAnyKeyPressed = false;

                if (instance.Plus.Observer is not null && instance.Plus.Observer.IsPressed)
                {
                    isResetting = false;
                    isAnyKeyPressed = true;
                    speed += instance.Plus.SpeedFunc(instance, instance.Plus.Observer);
                }

                if (instance.Minus.Observer is not null && instance.Minus.Observer.IsPressed)
                {
                    isResetting = false;
                    isAnyKeyPressed = true;
                    speed -= instance.Minus.SpeedFunc(instance, instance.Minus.Observer);
                }

                if (instance.Reset.Observer is not null && (isResetting || instance.Reset.Observer.IsPressed))
                {
                    isResetting = true;
                    speed -= sign * instance.Reset.SpeedFunc(instance, instance.Reset.Observer);
                }
                else if (!isAnyKeyPressed)
                {
                    speed -= sign * AutoReleaseSpeed;
                }

                float newValue = float.Clamp(instance.Value + speed * dt, instance.Min, instance.Max);
                return sign == -float.Sign(newValue - instance.Neutral) ? instance.Neutral : newValue;
            }
        }

        public AxisFactory SetInitialValue(double value)
        {
            InitialValue = (float)value;
            return this;
        }

        public AxisFactory BindPlus(Key key, ScriptAxis.SpeedFunc speedFunc)
        {
            Plus.Observer?.Dispose();
            Plus = new ScriptAxis.KeyBinding(Parent.InputManager.ObserveKey(key), speedFunc);
            return this;
        }

        public AxisFactory BindPlus(Key key, double engageSpeed, double releaseSpeed)
        {
            float floatEngageSpeed = (float)engageSpeed;
            float floatReleaseSpeed = (float)releaseSpeed;
            return BindPlus(key, (instance, observer) => instance.Neutral < instance.Value ? floatEngageSpeed : floatReleaseSpeed);
        }

        public AxisFactory BindPlus(Key key, double speed) => BindPlus(key, speed, speed);

        public AxisFactory BindPlus(string keyCode, ScriptAxis.SpeedFunc speedFunc)
        {
            return ParseKeyOrReport(keyCode, out Key key) ? BindPlus(key, speedFunc) : this;
        }

        public AxisFactory BindPlus(string keyCode, double forwardSpeed, double backwardSpeed)
        {
            return ParseKeyOrReport(keyCode, out Key key) ? BindPlus(key, forwardSpeed, backwardSpeed) : this;
        }

        public AxisFactory BindPlus(string keyCode, double speed) => BindPlus(keyCode, speed, speed);

        public AxisFactory BindMinus(Key key, ScriptAxis.SpeedFunc speedFunc)
        {
            Minus.Observer?.Dispose();
            Minus = new ScriptAxis.KeyBinding(Parent.InputManager.ObserveKey(key), speedFunc);
            return this;
        }

        public AxisFactory BindMinus(Key key, double engageSpeed, double releaseSpeed)
        {
            float floatEngageSpeed = (float)engageSpeed;
            float floatReleaseSpeed = (float)releaseSpeed;
            return BindMinus(key, (instance, observer) => instance.Value < instance.Neutral ? floatEngageSpeed : floatReleaseSpeed);
        }

        public AxisFactory BindMinus(Key key, double speed) => BindMinus(key, speed, speed);

        public AxisFactory BindMinus(string keyCode, ScriptAxis.SpeedFunc speedFunc)
        {
            return ParseKeyOrReport(keyCode, out Key key) ? BindMinus(key, speedFunc) : this;
        }

        public AxisFactory BindMinus(string keyCode, double forwardSpeed, double backwardSpeed)
        {
            return ParseKeyOrReport(keyCode, out Key key) ? BindMinus(key, forwardSpeed, backwardSpeed) : this;
        }

        public AxisFactory BindMinus(string keyCode, double speed) => BindMinus(keyCode, speed, speed);

        public AxisFactory BindReset(Key key, ScriptAxis.SpeedFunc speedFunc)
        {
            Reset.Observer?.Dispose();
            Reset = new ScriptAxis.KeyBinding(Parent.InputManager.ObserveKey(key), speedFunc);
            return this;
        }

        public AxisFactory BindReset(Key key, double speed)
        {
            float floatSpeed = (float)speed;
            return BindReset(key, (_, _) => floatSpeed);
        }

        public AxisFactory BindReset(string keyCode, ScriptAxis.SpeedFunc speedFunc)
        {
            return ParseKeyOrReport(keyCode, out Key key) ? BindReset(key, speedFunc) : this;
        }

        public AxisFactory BindReset(string keyCode, double speed)
        {
            return ParseKeyOrReport(keyCode, out Key key) ? BindReset(key, speed) : this;
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

        public AxisFactory AutoRelease(double speed)
        {
            AutoReleaseSpeed = (float)speed;
            return this;
        }

        public AxisFactory OnTick(ScriptAxis.TickFunc func)
        {
            OnTickFunc = func;
            return this;
        }

        public ScriptAxis Build()
        {
            BuiltAxis = new ScriptAxis(Key, Min, Neutral, Max, InitialValue)
            {
                Plus = Plus,
                Minus = Minus,
                Reset = Reset,
                OnTick = OnTickFunc,
            };
            Parent.AddAxis(BuiltAxis);
            return BuiltAxis;
        }
    }
}
