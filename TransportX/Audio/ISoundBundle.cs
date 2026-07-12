using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Audio
{
    public interface ISoundBundle : IDisposable
    {
        string Key { get; }
        IReadOnlyDictionary<string, ISoundAsset> Sounds { get; }
    }
}
