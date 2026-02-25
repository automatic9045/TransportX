using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Components
{
    public interface IComponentCollection : IReadOnlyCollection<IComponent>
    {
        T? Get<T>() where T : class, IComponent;
        bool TryGet<T>([MaybeNullWhen(false)] out T component) where T : class, IComponent;

        void Add(Type type, IComponent component);
        void Add<T>(T component) where T : class, IComponent;

        bool Remove(Type type);
        bool Remove<T>() where T : class, IComponent;

        void AddTo(IComponentCollection dest);
    }
}
