using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

namespace TransportX.Worlds
{
    public interface IWorldComponent : IComponent
    {
    }

    public interface IDisposableComponent : IWorldComponent, IDisposable
    {
    }

    public interface IStartableComponent : IWorldComponent
    {
        void OnStart();
    }

    public interface ISubTickableComponent : IWorldComponent
    {
        void SubTick(TimeSpan elapsed);
    }

    public interface ITickableComponent : IWorldComponent
    {
        void Tick(TimeSpan elapsed);
    }
}
