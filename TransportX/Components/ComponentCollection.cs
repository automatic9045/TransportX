using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Components
{
    public class ComponentCollection : IComponentCollection
    {
        private readonly ConcurrentDictionary<Type, IComponent> Components = [];

        public int Count => Components.Count;

        public bool TryGet<T>(out T component) where T : class, IComponent
        {
            if (Components.TryGetValue(typeof(T), out IComponent? baseComponent))
            {
                component = (T)baseComponent;
                return true;
            }

            component = null!;
            return false;
        }

        public T Get<T>() where T : class, IComponent
        {
            return TryGet(out T component) ? component
                : throw new KeyNotFoundException($"コンポーネント '{typeof(T)}' は登録されていません。");
        }

        private void AddUnchecked(Type type, IComponent component)
        {
            if (!Components.TryAdd(type, component))
            {
                throw new InvalidOperationException($"コンポーネント '{type}' は既に登録されています。");
            }
        }

        public void Add(Type type, IComponent component)
        {
            if (type.IsValueType || !typeof(IComponent).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"型 '{type}' は {typeof(IComponent)} を実装する参照型ではありません。");
            }

            Type componentType = component.GetType();
            if (!type.IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"{nameof(component)} パラメータの型 '{componentType}' は '{type}' と互換性がありません。", nameof(component));
            }

            AddUnchecked(type, component);
        }

        public void Add<T>(T component) where T : class, IComponent
        {
            AddUnchecked(typeof(T), component);
        }

        public bool Remove(Type type)
        {
            return Components.TryRemove(type, out _);
        }

        public bool Remove<T>() where T : class, IComponent
        {
            return Remove(typeof(T));
        }

        public void AddTo(IComponentCollection dest)
        {
            foreach ((Type type, IComponent component) in Components)
            {
                dest.Add(type, component);
            }
        }

        public IEnumerator<IComponent> GetEnumerator() => Components.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
