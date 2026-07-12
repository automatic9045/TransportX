using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Audio
{
    public class SoundBundle : ISoundBundle
    {
        public static SoundBundle Empty(string key) => new(key, new Dictionary<string, ISoundAsset>());


        public string Key { get; }
        public IReadOnlyDictionary<string, ISoundAsset> Sounds { get; }

        public SoundBundle(string key, IReadOnlyDictionary<string, ISoundAsset> sounds)
        {
            Key = key;
            Sounds = sounds;
        }

        public void Dispose()
        {
            foreach (ISoundAsset sound in Sounds.Values)
            {
                sound.Dispose();
            }
        }
    }
}
