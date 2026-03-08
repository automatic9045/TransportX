using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Components
{
    public class ComponentEngine : IDisposable
    {
        private readonly List<IComponent> All = [];
        private readonly List<IDisposableComponent> Disposables = [];
        private readonly List<IStartableComponent> Startables = [];
        private readonly List<ISubTickableComponent> SubTickables = [];
        private readonly List<ITickableComponent> Tickables = [];

        public ComponentEngine()
        {
        }

        public void Dispose()
        {
            foreach (IDisposableComponent component in Disposables) component.Dispose();
        }

        public void Register(IComponentCollection collection)
        {
            foreach (IComponent component in collection.Items) Add(component);

            collection.Added += OnAdded;
            collection.Removed += OnRemoved;
        }

        public void Unregister(IComponentCollection collection)
        {
            foreach (IComponent component in collection.Items) Remove(component);

            collection.Added -= OnAdded;
            collection.Removed -= OnRemoved;
        }

        private void OnAdded(object? sender, ComponentEventArgs<IComponent> e) => Add(e.Item);

        private void Add(IComponent item)
        {
            All.Add(item);
            if (item is IDisposableComponent disposable) Disposables.Add(disposable);
            if (item is IStartableComponent startable) Startables.Add(startable);
            if (item is ISubTickableComponent subTickable) SubTickables.Add(subTickable);
            if (item is ITickableComponent tickable) Tickables.Add(tickable);
        }

        private void OnRemoved(object? sender, ComponentEventArgs<IComponent> e) => Remove(e.Item);

        private void Remove(IComponent item)
        {
            All.Remove(item);
            if (item is IDisposableComponent disposable) FastRemove(Disposables, disposable);
            if (item is IStartableComponent startable) FastRemove(Startables, startable);
            if (item is ISubTickableComponent subTickable) FastRemove(SubTickables, subTickable);
            if (item is ITickableComponent tickable) FastRemove(Tickables, tickable);


            static void FastRemove<T>(List<T> list, T item)
            {
                int index = list.IndexOf(item);
                if (index < 0) throw new ArgumentException($"{nameof(item)} がリストに見つかりません。", nameof(item));

                int lastIndex = list.Count - 1;
                list[index] = list[lastIndex];
                list.RemoveAt(lastIndex);
            }
        }

        public void OnStart()
        {
            foreach (IStartableComponent component in Startables) component.OnStart();
        }

        public void SubTick(TimeSpan elapsed)
        {
            foreach (ISubTickableComponent component in SubTickables) component.SubTick(elapsed);
        }

        public void Tick(TimeSpan elapsed, DateTime now)
        {
            foreach (ITickableComponent component in Tickables) component.Tick(elapsed, now);
        }
    }
}
