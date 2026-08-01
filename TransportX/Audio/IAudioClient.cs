using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.XAudio2;

using TransportX.Spatial;

namespace TransportX.Audio
{
    public interface IAudioClient
    {
        Listener Listener { get; }
    }
}
