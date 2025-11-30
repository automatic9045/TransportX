using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Debug;
using Vortice.DXGI;
using Vortice.XAudio2;

using Bus.Common.Rendering;

namespace Bus.Models
{
    public class DXHost : IDXHost, IDisposable
    {
        private static readonly bool Debug = false;

        static DXHost()
        {
#if DEBUG
            Debug = true;
#endif
        }


        public ID3D11Device Device { get; } 
        public ID3D11DeviceContext Context { get; }
        public IDXGIFactory2 DXGIFactory { get; }
        public IXAudio2 XAudio2 { get; }
        public IXAudio2MasteringVoice MasteringVoice { get; }
        public X3DAudio X3DAudio { get; }

        public DXHost()
        {
            DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport;
            if (Debug) creationFlags |= DeviceCreationFlags.Debug;

            D3D11.D3D11CreateDevice(null, DriverType.Hardware, creationFlags, [ FeatureLevel.Level_11_1 ], out ID3D11Device device, out FeatureLevel featureLevel, out ID3D11DeviceContext context);
            Device = device;
            Context = context;

            DXGIFactory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(Debug);

            XAudio2 = Vortice.XAudio2.XAudio2.XAudio2Create();
            MasteringVoice = XAudio2.CreateMasteringVoice();
            X3DAudio = new X3DAudio(MasteringVoice.ChannelMask);
        }

        public void Dispose()
        {
            ID3D11Debug? d3dDebug = Debug ? Device.QueryInterface<ID3D11Debug>() : null;

            Device.Dispose();
            Context.Dispose();
            DXGIFactory.Dispose();
            MasteringVoice.Dispose();
            XAudio2.Dispose();

            d3dDebug?.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Detail | ReportLiveDeviceObjectFlags.IgnoreInternal);
            d3dDebug?.Dispose();
        }
    }
}
