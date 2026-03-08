using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Components
{
    public interface IComponentCollection
    {
        IEnumerable<IComponent> Items { get; }

        event EventHandler<ComponentEventArgs<IComponent>>? Added;
        event EventHandler<ComponentEventArgs<IComponent>>? Removed;
    }

    public interface IComponentCollection<TBase> : IComponentCollection, IReadOnlyDictionary<Type, TBase> where TBase : class, IComponent
    {
        IEnumerable<IComponent> IComponentCollection.Items => Values;

        new event EventHandler<ComponentEventArgs<TBase>>? Added;
        new event EventHandler<ComponentEventArgs<TBase>>? Removed;

        T? Get<T>() where T : class, TBase;
        bool TryGet<T>([MaybeNullWhen(false)] out T component) where T : class, TBase;

        void Add(Type type, TBase component);
        void Add<T>(T component) where T : class, TBase;
        T GetOrAdd<T>(Func<T> componentFactory) where T : class, TBase;

        bool Remove(Type type);
        bool Remove<T>() where T : class, TBase;
    }
}
