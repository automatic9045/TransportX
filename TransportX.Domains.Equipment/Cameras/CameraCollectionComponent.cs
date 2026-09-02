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
        private int RenderCount = 0;
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
                if (RenderCount % 4 != i % 4) continue;
                ISceneCaptureCamera camera = SceneCapture[i];

                Context.OMSetRenderTargets(camera.Surface.RenderTarget, camera.Surface.DepthStencil);
                Context.ClearRenderTargetView(camera.Surface.RenderTarget, Vortice.Mathematics.Colors.Black);

                ViewContext viewContext = camera.CreateViewContext(context.ViewContext);
                RenderPassContext captureContext = context with
                {
                    Surface = camera.Surface,
                    ViewContext = viewContext,
                    Flags = camera.RenderFlags,
                    OutputMode = RenderPassOutputMode.Forward,
                    ViewportSize = camera.RenderTarget.Size,
                };
                OpaquePass.Execute(captureContext, world);
            }

            RenderCount = unchecked(RenderCount + 1);
        }
    }
}
