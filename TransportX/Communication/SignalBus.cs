using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Communication
{
    public class SignalBus
    {
        private readonly List<Action> OnSubTick = [];

        public ConcurrentDictionary<string, Signal<float>> Single { get; } = [];
        public ConcurrentDictionary<string, Signal<int>> Int32 { get; } = [];
        public ConcurrentDictionary<string, Signal<bool>> Boolean { get; } = [];

        public SignalBus()
        {
        }

        public void Forward<T>(Signal<T> signal, Func<T> valueFactory)
        {
            OnSubTick.Add(() => signal.Value = valueFactory());
        }

        public void SubTick(TimeSpan elapsed)
        {
            foreach (Action action in OnSubTick)
            {
                action();
            }
        }
    }
}
