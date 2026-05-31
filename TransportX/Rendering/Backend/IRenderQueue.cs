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
        void Submit(IModel model, in InstanceData instance);
        void Render(in DrawContext context);
    }
}
