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

        public static Signal<int> ToSwitchCounter(this Signals signals, string intKey, string sourceBoolKey, bool targetValue)
        {
            Signal<int> signal = signals.Int(intKey);
            Signal<bool> sourceSignal = signals.Bool(sourceBoolKey);
            return signals.Source.ToSwitchCounter(signal, sourceSignal, targetValue);
        }
    }
}
