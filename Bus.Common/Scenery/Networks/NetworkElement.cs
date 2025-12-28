using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;

namespace Bus.Common.Scenery.Networks
{
    public abstract class NetworkElement : LocatableObject, IDrawable
    {
        public bool IsRoot { get; }

        public abstract NetworkPort.Inlet Inlet { get; }
        public abstract IReadOnlyList<NetworkPort> Outlets { get; }
        public IReadOnlyList<NetworkPort> Ports => [Inlet, ..Outlets];

        public virtual IReadOnlyList<LocatedModel> Models { get; } = [];

        public NetworkElement(int plateX, int plateZ, Matrix4x4 transform, bool isRoot) : base(plateX, plateZ, transform)
        {
            IsRoot = isRoot;
        }

        protected void SetChild(int outletIndex, NetworkElement element)
        {
            Outlets[outletIndex].SetChild(element);
        }

        public void Draw(LocatedDrawContext context)
        {
            foreach (LocatedModel model in Models)
            {
                model.Draw(context);
            }
        }
    }
}
