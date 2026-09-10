using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

namespace TransportX.Scripting.Input
{
    public class ScriptAxis : IAxis
    {
        private bool IsResetting = false;
        private float ActiveResetSpeed = 0;

        public string Key { get; }
        public float Min { get; }
        public float Neutral { get; }
        public float Max { get; }

        public required IReadOnlyList<KeyBinding> PlusBindings { get; init; }
        public required IReadOnlyList<KeyBinding> MinusBindings { get; init; }
        public required IReadOnlyList<KeyBinding> ResetBindings { get; init; }
        public required IReadOnlyList<JoystickAxisBinding> JoystickBindings { get; init; }

        public float AutoReleaseSpeed { get; init; } = 0;
        public TickFunc OnTick { get; init; }

        public float Value { get; private set; }

        internal ScriptAxis(string key, float min, float neutral, float max, float initialValue)
        {
            Key = key;

            Min = min;
            Neutral = neutral;
            Max = max;

            OnTick = TickDefault;

            Value = initialValue;
        }

        internal static ScriptAxis Empty(string key)
        {
            return new ScriptAxis(key, 0, 0, 0, 0)
            {
                PlusBindings = [],
                MinusBindings = [],
                ResetBindings = [],
                JoystickBindings = [],
                OnTick = (_, _) => 0,
            };
        }

        public void Dispose()
        {
            foreach (KeyBinding binding in PlusBindings) binding.Observer?.Dispose();
            foreach (KeyBinding binding in MinusBindings) binding.Observer?.Dispose();
            foreach (KeyBinding binding in ResetBindings) binding.Observer?.Dispose();
            foreach (JoystickAxisBinding binding in JoystickBindings) binding.Observer?.Dispose();
        }

        public void Tick(TimeSpan elapsed)
        {
            Value = OnTick(this, (float)elapsed.TotalSeconds);
        }

        public static float TickDefault(ScriptAxis instance, float dt)
        {
            int sign = float.Sign(instance.Value - instance.Neutral);

            float keyboardSpeed = 0;
            bool isAnyKeyPressed = false;

            if (TryGetKeyboardSpeed(instance.PlusBindings, out float plusSpeed))
            {
                instance.IsResetting = false;
                isAnyKeyPressed = true;
                keyboardSpeed += plusSpeed;
            }

            if (TryGetKeyboardSpeed(instance.MinusBindings, out float minusSpeed))
            {
                instance.IsResetting = false;
                isAnyKeyPressed = true;
                keyboardSpeed -= minusSpeed;
            }

            if (!isAnyKeyPressed)
            {
                for (int i = 0; i < instance.JoystickBindings.Count; i++)
                {
                    JoystickAxisBinding joystick = instance.JoystickBindings[i];
                    if (!joystick.Observer.IsConnected) continue;

                    float rawValue = joystick.Observer.Value;
                    float rawNeutral = joystick.RawNeutral;
                    if (joystick.IsInverted)
                    {
                        rawValue = joystick.RawMax - (rawValue - joystick.RawMin);
                        rawNeutral = joystick.RawMax - (rawNeutral - joystick.RawMin);
                    }
                    rawValue = float.Clamp(rawValue, joystick.RawMin, joystick.RawMax);
                    rawNeutral = float.Clamp(rawNeutral, joystick.RawMin, joystick.RawMax);

                    instance.IsResetting = false;

                    if (rawValue < rawNeutral)
                    {
                        float range = joystick.RawNeutral - joystick.RawMin;
                        float rate = 0 < range ? (rawValue - joystick.RawMin) / range : 0;
                        return float.Lerp(instance.Min, instance.Neutral, rate);
                    }
                    else
                    {
                        float range = joystick.RawMax - joystick.RawNeutral;
                        float rate = 0 < range ? (rawValue - joystick.RawNeutral) / range : 0;
                        return float.Lerp(instance.Neutral, instance.Max, rate);
                    }
                }
            }

            bool isResetKeyPressed = TryGetKeyboardSpeed(instance.ResetBindings, out float resetSpeed);
            if (isResetKeyPressed)
            {
                instance.ActiveResetSpeed = resetSpeed;
            }

            if (0 < instance.ResetBindings.Count && (instance.IsResetting || isResetKeyPressed))
            {
                instance.IsResetting = true;
                isAnyKeyPressed = true;

                if (!isResetKeyPressed)
                {
                    resetSpeed = instance.ActiveResetSpeed;
                }

                keyboardSpeed -= sign * resetSpeed;
            }

            if (!isAnyKeyPressed)
            {
                keyboardSpeed -= sign * instance.AutoReleaseSpeed;
            }

            float newValue = float.Clamp(instance.Value + keyboardSpeed * dt, instance.Min, instance.Max);

            if (!isAnyKeyPressed || instance.IsResetting)
            {
                if (sign == -float.Sign(newValue - instance.Neutral))
                {
                    newValue = instance.Neutral;
                    instance.IsResetting = false;
                }
            }

            return newValue;


            bool TryGetKeyboardSpeed(IReadOnlyList<KeyBinding> bindings, out float speed)
            {
                speed = 0;

                bool isBound = false;
                for (int i = 0; i < bindings.Count; i++)
                {
                    KeyBinding binding = bindings[i];
                    if (binding.Observer is not null && binding.Observer.IsPressed)
                    {
                        float speedCandidate = binding.SpeedFunc(instance, binding.Observer);
                        if (!isBound || float.Abs(speed) < float.Abs(speedCandidate))
                        {
                            speed = speedCandidate;
                            isBound = true;
                        }
                    }
                }

                return isBound;
            }
        }


        public readonly record struct KeyBinding(KeyObserver? Observer, SpeedFunc SpeedFunc);
        public readonly record struct JoystickAxisBinding(IJoystickAxisObserver Observer, int RawMin, int RawNeutral, int RawMax, bool IsInverted);

        public delegate float SpeedFunc(ScriptAxis instance, KeyObserver observer);
        public delegate float TickFunc(ScriptAxis instance, float dt);
    }
}
