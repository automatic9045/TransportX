using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

namespace TransportX.Collections
{
    public interface IModelCollection : IDisposable
    {
        IReadOnlyKeyedList<string, IModelBundle> Bundles { get; }

        IModel GetModel(string modelKey);
        bool TryGetModel(string modelKey, [MaybeNullWhen(false)] out IModel model);

        bool AdoptBundle(IModelBundle bundle, bool allowOverride = false);
        bool ReleaseBundle(string bundleKey);
    }
}
