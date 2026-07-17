using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;

namespace TransportX.Scripting
{
    public class TickState<T> : ITickState<T> where T : notnull
    {
        private readonly IEqualityComparer<T> EqualityComparer;

        private Action OnEnterAction = () => { };
        private Action<TimeSpan, TimeSpan> OnTickAction = (elapsed, stateTime) => { };
        private Action OnExitAction = () => { };
        private Func<TimeSpan, TimeSpan, T> EvaluateTransitionFunc;

        public T Key { get; }

        public TickState(T state, IEqualityComparer<T> equalityComparer)
        {
            Key = state;
            EqualityComparer = equalityComparer;

            EvaluateTransitionFunc = (elapsed, stateTime) => Key;
        }

        public static TickState<T> Create(T state) => new(state, EqualityComparer<T>.Default);

        public TickState<T> OnEnter(Action action)
        {
            OnEnterAction = action;
            return this;
        }

        public TickState<T> OnTick(Action<TimeSpan, TimeSpan> action)
        {
            OnTickAction = action;
            return this;
        }

        public TickState<T> OnExit(Action action)
        {
            OnExitAction = action;
            return this;
        }

        public TickState<T> EvaluateTransition(Func<TimeSpan, TimeSpan, T> func)
        {
            EvaluateTransitionFunc = func;
            return this;
        }

        void ITickState<T>.OnEnter() => OnEnterAction();
        void ITickState<T>.OnTick(TimeSpan elapsed, TimeSpan stateTime) => OnTickAction(elapsed, stateTime);
        void ITickState<T>.OnExit() => OnExitAction();
        bool ITickState<T>.EvaluateTransition(TimeSpan elapsed, TimeSpan stateTime, out T next)
        {
            next = EvaluateTransitionFunc(elapsed, stateTime);
            return !EqualityComparer.Equals(next, Key);
        }
    }
}
