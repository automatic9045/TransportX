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
        private static readonly bool IsDebug = false;

        static DXHost()
        {
#if DEBUG
            IsDebug = true;
#endif
        }


        public ID3D11Device Device { get; } 
        public ID3D11DeviceContext Context { get; }
        public ID3D11Debug? Debug { get; }
        public IDXGIFactory2 DXGIFactory { get; }
        public IXAudio2 XAudio2 { get; }
        public IXAudio2MasteringVoice MasteringVoice { get; }
        public X3DAudio X3DAudio { get; }

        public event EventHandler? Disposing;

        public DXHost()
        {
            DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport;
            if (IsDebug) creationFlags |= DeviceCreationFlags.Debug;

            D3D11.D3D11CreateDevice(null, DriverType.Hardware, creationFlags, [ FeatureLevel.Level_11_1 ], out ID3D11Device device, out FeatureLevel featureLevel, out ID3D11DeviceContext context);
            Device = device;
            Context = context;
            Debug = IsDebug ? Device.QueryInterface<ID3D11Debug>() : null;

            DXGIFactory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(IsDebug);

            XAudio2 = Vortice.XAudio2.XAudio2.XAudio2Create();
            MasteringVoice = XAudio2.CreateMasteringVoice();
            X3DAudio = new X3DAudio(MasteringVoice.ChannelMask);
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);

            DXGIFactory.Dispose();

            MasteringVoice.Dispose();
            XAudio2.Dispose();

            Context.ClearState();
            Context.Flush();
            Context.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Device.Dispose();

            Debug?.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Detail);
            Debug?.Dispose();
        }
    }
}
