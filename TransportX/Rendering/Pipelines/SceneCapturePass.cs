using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Worlds;

namespace TransportX.Rendering.Pipelines
{
    public class SceneCapturePass : IRenderPass
    {
        private readonly RenderResourceSet Resources;

        private ISceneCaptureComponent[] Components = [];

        public SceneCapturePass(RenderResourceSet resources)
        {
            Resources = resources;
        }

        public void Dispose()
        {
        }

        public void InitializeComponents(IEnumerable<ISceneCaptureComponent> components)
        {
            Components = components.ToArray();
            for (int i = 0; i < Components.Length; i++)
            {
                Components[i].Initialize(Resources);
            }
        }

        public void Execute(in RenderPassContext context, WorldBase world)
        {
            for (int i = 0; i < Components.Length; i++)
            {
                Components[i].Capture(context, world);
            }
        }
    }
}
