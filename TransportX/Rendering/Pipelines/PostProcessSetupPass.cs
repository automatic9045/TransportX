using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Spatial;
using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public class PostProcessSetupPass : IRenderPass
    {
        private readonly RenderResourceSet Resources;
        private readonly PostProcessPass PostProcess;

        public PostProcessSetupPass(RenderResourceSet resources, PostProcessPass postProcess)
        {
            Resources = resources;
            PostProcess = postProcess;
        }

        public void Dispose()
        {
        }

        public void Execute(in RenderPassContext context, WorldBase world)
        {
            if (context.ViewportSize.Width == 0 || context.ViewportSize.Height == 0)
            {
                PostProcess.Reset();
            }

            PostProcess.Setup(context.Surface.DepthStencil!, context.ViewportSize);

            ID3D11DeviceContext deviceContext = Resources.Context.DeviceContext;
            deviceContext.RSSetViewport(0, 0, context.ViewportSize.Width, context.ViewportSize.Height);
            deviceContext.ClearDepthStencilView(context.Surface.DepthStencil!, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
        }
    }
}
