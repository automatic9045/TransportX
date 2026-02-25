using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Components
{
    public interface IComponentCollection<TBase> : IReadOnlyCollection<TBase> where TBase : class, IComponent
    {
        T? Get<T>() where T : class, TBase;
        bool TryGet<T>([MaybeNullWhen(false)] out T component) where T : class, TBase;

        void Add(Type type, TBase component);
        void Add<T>(T component) where T : class, TBase;

        bool Remove(Type type);
        bool Remove<T>() where T : class, TBase;

        void AddTo(IComponentCollection<TBase> dest);
    }
}
