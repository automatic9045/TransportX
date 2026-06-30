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
        public string Key { get; }
        public float Min { get; }
        public float Neutral { get; }
        public float Max { get; }

        public required KeyBinding Plus { get; init; }
        public required KeyBinding Minus { get; init; }
        public required KeyBinding Reset { get; init; }

        public required TickFunc OnTick { get; init; }

        public float Value { get; private set; }

        internal ScriptAxis(string key, float min, float neutral, float max, float initialValue)
        {
            Key = key;

            Min = min;
            Neutral = neutral;
            Max = max;

            Value = initialValue;
        }

        internal static ScriptAxis Empty(string key)
        {
            return new ScriptAxis(key, 0, 0, 0, 0)
            {
                Plus = default,
                Minus = default,
                Reset = default,
                OnTick = (_, _) => 0,
            };
        }

        public void Dispose()
        {
            Plus.Observer?.Dispose();
            Minus.Observer?.Dispose();
            Reset.Observer?.Dispose();
        }

        public void Tick(TimeSpan elapsed)
        {
            Value = OnTick(this, (float)elapsed.TotalSeconds);
        }


        public readonly record struct KeyBinding(KeyObserver? Observer, SpeedFunc SpeedFunc);
        
        public delegate float SpeedFunc(ScriptAxis instance, KeyObserver observer);
        public delegate float TickFunc(ScriptAxis instance, float dt);
    }
}
