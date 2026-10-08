using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

namespace TransportX.Scripting.Commands
{
    public static class SignalsExtensions
    {
        public static Signal<bool> ToToggle(this Signals signals, string boolKey, string sourceBoolKey, bool initialValue)
        {
            Signal<bool> signal = signals.Bool(boolKey);
            Signal<bool> sourceSignal = signals.Bool(sourceBoolKey);
            return signals.Source.ToToggle(signal, sourceSignal, initialValue);
        }

        public static Signal<int> ToFloatSwitchCounter(this Signals signals, string intKey, string sourceFloatKey, float targetValue)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<float> sourceSignal = signals.Float(sourceFloatKey);
            return signals.Source.ToSwitchCounter(signal, sourceSignal, targetValue);
        }

        public static Signal<int> ToIntSwitchCounter(this Signals signals, string intKey, string sourceIntKey, int targetValue)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<int> sourceSignal = signals.Int(sourceIntKey);
            return signals.Source.ToSwitchCounter(signal, sourceSignal, targetValue);
        }

        public static Signal<int> ToBoolSwitchCounter(this Signals signals, string intKey, string sourceBoolKey, bool targetValue)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<bool> sourceSignal = signals.Bool(sourceBoolKey);
            return signals.Source.ToSwitchCounter(signal, sourceSignal, targetValue);
        }

        public static Signal<int> ToFloatSwitchFromCounter(this Signals signals, string intKey, string sourceFloatKey, float targetValue)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<float> sourceSignal = signals.Float(sourceFloatKey);
            return signals.Source.ToSwitchFromCounter(signal, sourceSignal, targetValue);
        }

        public static Signal<int> ToIntSwitchFromCounter(this Signals signals, string intKey, string sourceIntKey, int targetValue)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<int> sourceSignal = signals.Int(sourceIntKey);
            return signals.Source.ToSwitchFromCounter(signal, sourceSignal, targetValue);
        }

        public static Signal<int> ToBoolSwitchFromCounter(this Signals signals, string intKey, string sourceBoolKey, bool targetValue)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<bool> sourceSignal = signals.Bool(sourceBoolKey);
            return signals.Source.ToSwitchFromCounter(signal, sourceSignal, targetValue);
        }

        public static Signal<int> ToFloatChangeCounter(this Signals signals, string intKey, string sourceFloatKey)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<float> sourceSignal = signals.Float(sourceFloatKey);
            return signals.Source.ToChangeCounter(signal, sourceSignal);
        }

        public static Signal<int> ToIntChangeCounter(this Signals signals, string intKey, string sourceIntKey)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<int> sourceSignal = signals.Int(sourceIntKey);
            return signals.Source.ToChangeCounter(signal, sourceSignal);
        }

        public static Signal<int> ToBoolChangeCounter(this Signals signals, string intKey, string sourceBoolKey)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<bool> sourceSignal = signals.Bool(sourceBoolKey);
            return signals.Source.ToChangeCounter(signal, sourceSignal);
        }
    }
}
