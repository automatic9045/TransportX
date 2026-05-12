using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX
{
    public abstract record AppReference
    {
        public static TypeAppReference FromType<TFactory>() where TFactory : IAppFactory => new(typeof(TFactory));
        public static TypeAppReference FromType(Type factoryType) => new(factoryType);
        public static PathAppReference FromPath(string path, string? factoryTypeFullName) => new(path, factoryTypeFullName);
    }

    public record TypeAppReference(Type FactoryType) : AppReference;
    public record PathAppReference(string Path, string? FactoryTypeFullName) : AppReference;
}
