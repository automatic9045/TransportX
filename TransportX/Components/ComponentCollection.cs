using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Components
{
    public class ComponentCollection<TBase> : IComponentCollection<TBase> where TBase : class, IComponent
    {
        private readonly ConcurrentDictionary<Type, TBase> Components = [];

        public int Count => Components.Count;

        public IEnumerable<Type> Keys => Components.Keys;
        public IEnumerable<TBase> Values => Components.Values;

        public TBase this[Type key] => Components[key];

        public event EventHandler<ComponentEventArgs<TBase>>? Added;
        public event EventHandler<ComponentEventArgs<TBase>>? Removed;

        protected event EventHandler<ComponentEventArgs<IComponent>>? NonGenericAdded;
        protected event EventHandler<ComponentEventArgs<IComponent>>? NonGenericRemoved;

        event EventHandler<ComponentEventArgs<IComponent>>? IComponentCollection.Added
        {
            add => NonGenericAdded += value;
            remove => NonGenericAdded -= value;
        }

        event EventHandler<ComponentEventArgs<IComponent>>? IComponentCollection.Removed
        {
            add => NonGenericRemoved += value;
            remove => NonGenericRemoved -= value;
        }

        public ComponentCollection()
        {
        }

        public bool TryGet<T>(out T component) where T : class, TBase
        {
            if (Components.TryGetValue(typeof(T), out TBase? baseComponent))
            {
                component = (T)baseComponent;
                return true;
            }

            component = null!;
            return false;
        }

        public T Get<T>() where T : class, TBase
        {
            return TryGet(out T component) ? component
                : throw new KeyNotFoundException($"コンポーネント '{typeof(T)}' は登録されていません。");
        }

        private void InvokeAdded(TBase component)
        {
            Added?.Invoke(this, new ComponentEventArgs<TBase>(component));
            NonGenericAdded?.Invoke(this, new ComponentEventArgs<IComponent>(component));
        }

        private void AddUnchecked(Type type, TBase component)
        {
            if (!Components.TryAdd(type, component))
            {
                throw new InvalidOperationException($"コンポーネント '{type}' は既に登録されています。");
            }

            InvokeAdded(component);
        }

        public void Add(Type type, TBase component)
        {
            if (type.IsValueType || !typeof(TBase).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"型 '{type}' は {typeof(TBase)} を実装する参照型ではありません。");
            }

            Type componentType = component.GetType();
            if (!type.IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"{nameof(component)} パラメータの型 '{componentType}' は '{type}' と互換性がありません。", nameof(component));
            }

            AddUnchecked(type, component);
        }

        public void Add<T>(T component) where T : class, TBase
        {
            AddUnchecked(typeof(T), component);
        }

        public T GetOrAdd<T>(Func<T> componentFactory) where T : class, TBase
        {
            return (T)Components.GetOrAdd(typeof(T), _ =>
            {
                T component = componentFactory();
                InvokeAdded(component);
                return component;
            });
        }

        public bool Remove(Type type)
        {
            if (Components.TryRemove(type, out TBase? component))
            {
                Removed?.Invoke(this, new ComponentEventArgs<TBase>(component));
                NonGenericRemoved?.Invoke(this, new ComponentEventArgs<IComponent>(component));
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove<T>() where T : class, TBase
        {
            return Remove(typeof(T));
        }

        public bool ContainsKey(Type key) => Components.ContainsKey(key);
        public bool TryGetValue(Type key, [MaybeNullWhen(false)] out TBase value) => Components.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<Type, TBase>> GetEnumerator() => Components.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
