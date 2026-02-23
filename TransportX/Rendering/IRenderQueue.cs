using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    public interface IRenderQueue
    {
        void Clear();
        void Submit(RenderPass pass, IModel model, in InstanceData instance);
        void Render(RenderPass pass, in DrawContext context);
    }
}
