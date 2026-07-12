using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;

namespace TransportX.Collections
{
    public interface ISoundCollection : IDisposable
    {
        IReadOnlyKeyedList<string, ISoundBundle> Bundles { get; }

        ISoundAsset GetSound(string soundKey);
        bool TryGetSound(string modelKey, [MaybeNullWhen(false)] out ISoundAsset sound);

        bool AdoptBundle(ISoundBundle bundle, bool allowOverride = false);
        bool ReleaseBundle(string bundleKey);
    }
}
