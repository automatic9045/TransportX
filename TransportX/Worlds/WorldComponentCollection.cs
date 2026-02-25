using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

namespace TransportX.Worlds
{
    public class WorldComponentCollection : ComponentCollection<IWorldComponent>, IDisposable
    {
        private readonly List<IDisposableComponent> Disposables = [];
        private readonly List<IStartableComponent> Startables = [];
        private readonly List<ISubTickableComponent> SubTickables = [];
        private readonly List<ITickableComponent> Tickables = [];

        public WorldComponentCollection()
        {
            Added += (sender, e) =>
            {
                if (e.Item is IDisposableComponent disposable) Disposables.Add(disposable);
                if (e.Item is IStartableComponent startable) Startables.Add(startable);
                if (e.Item is ISubTickableComponent subTickable) SubTickables.Add(subTickable);
                if (e.Item is ITickableComponent tickable) Tickables.Add(tickable);
            };

            Removed += (sender, e) =>
            {
                if (e.Item is IDisposableComponent disposable) Disposables.Remove(disposable);
                if (e.Item is IStartableComponent startable) Startables.Remove(startable);
                if (e.Item is ISubTickableComponent subTickable) SubTickables.Remove(subTickable);
                if (e.Item is ITickableComponent tickable) Tickables.Remove(tickable);
            };
        }

        public void Dispose()
        {
            foreach (IDisposableComponent component in Disposables) component.Dispose();
        }

        public void OnStart()
        {
            foreach (IStartableComponent component in Startables) component.OnStart();
        }

        public void SubTick(TimeSpan elapsed)
        {
            foreach (ISubTickableComponent component in SubTickables) component.SubTick(elapsed);
        }

        public void Tick(TimeSpan elapsed)
        {
            foreach (ITickableComponent component in Tickables) component.Tick(elapsed);
        }
    }
}
