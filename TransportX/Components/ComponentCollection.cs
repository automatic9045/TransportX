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

        public void Add<T>(T component) where T : class, IComponent
        {
            if (!Components.TryAdd(typeof(T), component))
            {
                throw new InvalidOperationException($"コンポーネント '{typeof(T)}' は既に登録されています。");
            }
        }

        public bool Remove<T>() where T : class, IComponent
        {
            return Components.TryRemove(typeof(T), out _);
        }

        public IEnumerator<IComponent> GetEnumerator() => Components.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
