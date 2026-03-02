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

        private void AddUnchecked(Type type, TBase component)
        {
            if (!Components.TryAdd(type, component))
            {
                throw new InvalidOperationException($"コンポーネント '{type}' は既に登録されています。");
            }

            Added?.Invoke(this, new ComponentEventArgs<TBase>(component));
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

        public bool Remove(Type type)
        {
            if (Components.TryRemove(type, out TBase? compoennt))
            {
                Removed?.Invoke(this, new ComponentEventArgs<TBase>(compoennt));
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
        public IEnumerator<KeyValuePair<Type, TBase>> GetEnumerator()=> Components.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
