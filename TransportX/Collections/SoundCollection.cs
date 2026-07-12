using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;

namespace TransportX.Collections
{
    public class SoundCollection : ISoundCollection
    {
        private readonly Dictionary<string, ISoundAsset> Sounds = [];
        private readonly KeyedList<string, ISoundBundle> BundlesKey = new(bundle => bundle.Key);

        public IReadOnlyKeyedList<string, ISoundBundle> Bundles => BundlesKey;

        public SoundCollection()
        {
        }

        public void Dispose()
        {
            foreach (ISoundBundle bundle in Bundles)
            {
                bundle.Dispose();
            }
        }

        public ISoundAsset GetSound(string soundKey)
        {
            return soundKey == string.Empty ? ISoundAsset.Empty : Sounds[soundKey];
        }

        public bool TryGetSound(string soundKey, [MaybeNullWhen(false)] out ISoundAsset sound)
        {
            if (soundKey == string.Empty)
            {
                sound = ISoundAsset.Empty;
                return true;
            }
            else
            {
                return Sounds.TryGetValue(soundKey, out sound);
            }
        }

        public bool AdoptBundle(ISoundBundle bundle, bool allowOverride = false)
        {
            if (Bundles.TryGetValue(bundle.Key, out ISoundBundle? existingBundle))
            {
                if (existingBundle == bundle)
                {
                    throw new ArgumentException("指定したサウンドバンドルは既に登録されています。", nameof(bundle));
                }

                if (allowOverride)
                {
                    ReleaseBundle(bundle.Key);
                }
                else
                {
                    throw new ArgumentException($"キー '{bundle.Key}' のサウンドバンドルは既に存在します。", nameof(bundle));
                }
            }

            BundlesKey.Add(bundle);

            foreach ((string modelKey, ISoundAsset model) in bundle.Sounds)
            {
                Sounds.Add(modelKey, model);
            }

            return true;
        }

        public bool ReleaseBundle(string bundleKey)
        {
            ISoundBundle bundle = Bundles[bundleKey];

            foreach (string modelKey in bundle.Sounds.Keys)
            {
                Sounds.Remove(modelKey);
            }

            BundlesKey.Remove(bundleKey);
            bundle.Dispose();

            return true;
        }
    }
}
