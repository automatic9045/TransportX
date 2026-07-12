using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;
using TransportX.Collections;
using TransportX.Diagnostics;

namespace TransportX.Scripting.Collections
{
    public class ScriptSoundCollection : ISoundCollection
    {
        private readonly IErrorCollector ErrorCollector;

        private readonly ScriptDictionary<string, ISoundAsset> Sounds;
        private readonly ScriptKeyedList<string, ISoundBundle> BundlesKey;

        public IReadOnlyKeyedList<string, ISoundBundle> Bundles => BundlesKey;

        public ScriptSoundCollection(IErrorCollector errorCollector)
        {
            ErrorCollector = errorCollector;

            Sounds = new ScriptDictionary<string, ISoundAsset>(errorCollector, "サウンド", key => ISoundAsset.Empty);
            BundlesKey = new ScriptKeyedList<string, ISoundBundle>(bundle => bundle.Key, errorCollector, "サウンドバンドル", SoundBundle.Empty);
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
                    ScriptError error = new(ErrorLevel.Error, "指定したサウンドバンドルは既に登録されています。");
                    ErrorCollector.Report(error);
                    return false;
                }

                if (allowOverride)
                {
                    ReleaseBundle(bundle.Key);
                }
                else
                {
                    ScriptError error = new(ErrorLevel.Error, $"キー '{bundle.Key}' のサウンドバンドルは既に存在します。");
                    ErrorCollector.Report(error);
                    return false;
                }
            }

            BundlesKey.Add(bundle);

            foreach ((string modelKey, ISoundAsset sound) in bundle.Sounds)
            {
                Sounds.Add(modelKey, sound);
            }

            return true;
        }

        public bool ReleaseBundle(string bundleKey)
        {
            if (!BundlesKey.GetValue(bundleKey, out ISoundBundle? bundle)) return false;

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
