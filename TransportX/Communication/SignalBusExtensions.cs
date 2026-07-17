using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Communication
{
    public static class SignalBusExtensions
    {
        public static Signal<bool> ToToggle(this SignalBus signalBus, Signal<bool> signal, Signal<bool> sourceSignal, bool initialValue)
        {
            bool lastCondition = false;
            signalBus.Forward(signal, () =>
            {
                bool condition = sourceSignal.Value;
                bool oldValue = signal.Value;
                bool newValue = !lastCondition && condition ? !oldValue : oldValue;
                lastCondition = condition;
                return newValue;
            });

            return signal;
        }

        public static Signal<int> ToSwitchCounter(this SignalBus signalBus, Signal<int> signal, Signal<bool> sourceSignal, bool targetValue)
        {
            bool lastCondition = sourceSignal.Value;
            signalBus.Forward(signal, () =>
            {
                bool condition = sourceSignal.Value;
                int newValue = lastCondition != targetValue && condition == targetValue ? unchecked(signal.Value + 1) : signal.Value;
                if (lastCondition != targetValue && condition == targetValue)
                {
                    int a = 0;
                    a = a + 2;
                }
                lastCondition = condition;
                return newValue;
            });

            return signal;
        }
    }
}
