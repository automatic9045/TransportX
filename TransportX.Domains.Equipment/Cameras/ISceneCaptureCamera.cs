using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering.Backend;
using TransportX.Rendering.Pipelines;
using TransportX.Spatial;

namespace TransportX.Domains.Equipment.Cameras
{
    public interface ISceneCaptureCamera : IDisposable
    {
        IWorldObject AttachedTo { get; }

        RenderTexture RenderTarget { get; }
        DepthTexture DepthTarget { get; }
        RenderSurface Surface => new(RenderTarget.RenderTargetView, DepthTarget.DepthStencilView);

        RenderPassFlags RenderFlags { get; }

        ViewContext CreateViewContext(in ViewContext baseViewContext);
    }
}
