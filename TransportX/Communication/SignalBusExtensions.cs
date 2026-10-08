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
            bool lastCondition = initialValue;
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

        public static Signal<int> ToSwitchCounter<TSource>(this SignalBus signalBus, Signal<int> signal, Signal<TSource> sourceSignal, TSource targetValue)
        {
            TSource lastValue = sourceSignal.Value;
            signalBus.Forward(signal, () =>
            {
                TSource value = sourceSignal.Value;
                int newValue = !EqualityComparer<TSource>.Default.Equals(lastValue, targetValue) && EqualityComparer<TSource>.Default.Equals(value, targetValue)
                    ? unchecked(signal.Value + 1) : signal.Value;
                lastValue = value;
                return newValue;
            });

            return signal;
        }

        public static Signal<int> ToSwitchFromCounter<TSource>(this SignalBus signalBus, Signal<int> signal, Signal<TSource> sourceSignal, TSource targetValue)
        {
            TSource lastValue = sourceSignal.Value;
            signalBus.Forward(signal, () =>
            {
                TSource value = sourceSignal.Value;
                int newValue = EqualityComparer<TSource>.Default.Equals(lastValue, targetValue) && !EqualityComparer<TSource>.Default.Equals(value, targetValue)
                    ? unchecked(signal.Value + 1) : signal.Value;
                lastValue = value;
                return newValue;
            });

            return signal;
        }

        public static Signal<int> ToChangeCounter<TSource>(this SignalBus signalBus, Signal<int> signal, Signal<TSource> sourceSignal)
        {
            TSource lastValue = sourceSignal.Value;
            signalBus.Forward(signal, () =>
            {
                TSource value = sourceSignal.Value;
                int newValue = !EqualityComparer<TSource>.Default.Equals(value, lastValue) ? unchecked(signal.Value + 1) : signal.Value;
                lastValue = value;
                return newValue;
            });

            return signal;
        }
    }
}
