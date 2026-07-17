using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Mathematics
{
    public interface ITickState<T> where T : notnull
    {
        T Key { get; }

        void OnEnter();
        void OnTick(TimeSpan elapsed, TimeSpan stateTime);
        void OnExit();

        bool EvaluateTransition(TimeSpan elapsed, TimeSpan stateTime, out T next);
    }
}
