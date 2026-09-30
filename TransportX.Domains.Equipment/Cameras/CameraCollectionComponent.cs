using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Components;
using TransportX.Rendering;
using TransportX.Rendering.Pipelines;
using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX.Domains.Equipment.Cameras
{
    public class CameraCollectionComponent : IDisposableComponent, ISceneCaptureComponent
    {
        private uint RenderCount = 0;
        private ID3D11DeviceContext? Context = null;
        private OpaquePass? OpaquePass = null;

        public List<ISceneCaptureCamera> SceneCapture { get; } = [];

        public CameraCollectionComponent()
        {
        }

        public void Dispose()
        {
            foreach (ISceneCaptureCamera camera in SceneCapture)
            {
                camera.Dispose();
            }

            OpaquePass?.Dispose();
        }

        public void Initialize(in RenderResourceSet resources)
        {
            Context = resources.Context.DeviceContext;
            OpaquePass = new OpaquePass(resources);
        }

        public void Capture(in RenderPassContext context, WorldBase world)
        {
            if (Context is null) throw new InvalidOperationException();
            if (OpaquePass is null) throw new InvalidOperationException();

            for (int i = 0; i < SceneCapture.Count; i++)
            {
                ISceneCaptureCamera camera = SceneCapture[i];

                float maxScreenRatio = camera.MaxScreenRatio.Consume(0);
                int interval = maxScreenRatio switch
                {
                    float x when 0.1f < x => 3,
                    float x when 0.04f < x => 6,
                    float x when 0.02f < x => 12,
                    float x when 0.015f < x => 15,
                    _ => 0,
                };
                if (interval == 0 || (interval != 1 && RenderCount % interval != i % interval)) continue;

                Context.OMSetRenderTargets(camera.Surface.RenderTarget, camera.Surface.DepthStencil);
                Context.ClearRenderTargetView(camera.Surface.RenderTarget, Vortice.Mathematics.Colors.Black);

                ViewContext viewContext = camera.CreateViewContext(context.ViewContext);
                RenderPassContext captureContext = context with
                {
                    Surface = camera.Surface,
                    ViewContext = viewContext,
                    Flags = camera.RenderFlags,
                    OutputMode = RenderPassOutputMode.Forward,
                    Purpose = ISceneCaptureCamera.Purpose,
                    ViewportSize = camera.RenderTarget.Size,
                };
                OpaquePass.Execute(captureContext, world);
            }

            RenderCount = unchecked(RenderCount + 1);
        }
    }
}
