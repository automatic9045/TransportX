using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Components
{
    public interface IComponent
    {
    }

    public interface IDisposableComponent : IComponent, IDisposable
    {
    }

    public interface IStartableComponent : IComponent
    {
        void OnStart();
    }

    public interface ISubTickableComponent : IComponent
    {
        void SubTick(TimeSpan elapsed);
    }

    public interface ITickableComponent : IComponent
    {
        void Tick(TimeSpan elapsed, DateTime now);
    }
}
