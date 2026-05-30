using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Backend
{
    public interface IRenderQueue
    {
        void Clear();
        void Submit(RenderLayer layer, IModel model, in InstanceData instance);
        void Render(RenderLayer layer, in DrawContext context);
    }
}
