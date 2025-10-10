using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.XAudio2;

namespace Bus.Common.Rendering
{
    public interface IDXHost
    {
        ID3D11Device Device { get; }
        ID3D11DeviceContext Context { get; }
        IDXGIFactory2 DXGIFactory { get; }
        IXAudio2 XAudio2 { get; }
        IXAudio2MasteringVoice MasteringVoice { get; }
        X3DAudio X3DAudio { get; }
    }
}
