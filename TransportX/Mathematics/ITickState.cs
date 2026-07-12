using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public interface ITickState<TState> where TState : struct
    {
        void OnEnter();
        void OnTick(TimeSpan elapsed, TimeSpan stateTime);
        void OnExit();

        bool EvaluateTransition(TimeSpan elapsed, TimeSpan stateTime, out TState next);
    }
}
