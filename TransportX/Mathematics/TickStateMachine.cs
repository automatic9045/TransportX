using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;

namespace TransportX.Mathematics
{
    public class TickStateMachine<TState> where TState : struct
    {
        private readonly KeyedList<TState, ITickState<TState>> States = new(state => state.Key);

        public TState State { get; private set; }
        public TimeSpan StateTime { get; private set; } = TimeSpan.Zero;

        public TickStateMachine(TState initialState)
        {
            State = initialState;
        }

        public void AddState(ITickState<TState> state)
        {
            States.Add(state);
        }

        public void TransitionTo(TState next)
        {
            if (States.TryGetValue(State, out ITickState<TState>? currentState))
            {
                currentState.OnExit();
            }

            State = next;
            StateTime = TimeSpan.Zero;

            if (States.TryGetValue(State, out ITickState<TState>? nextState))
            {
                nextState.OnEnter();
            }
        }

        public void Tick(TimeSpan elapsed)
        {
            StateTime += elapsed;

            if (States.TryGetValue(State, out ITickState<TState>? state))
            {
                if (state.EvaluateTransition(elapsed, StateTime, out TState next) && !EqualityComparer<TState>.Default.Equals(next, State))
                {
                    TransitionTo(next);
                    return;
                }

                state.OnTick(elapsed, StateTime);
            }
        }
    }
}
