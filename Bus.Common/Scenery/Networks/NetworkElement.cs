using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Collections;
using Bus.Common.Rendering;

namespace Bus.Common.Scenery.Networks
{
    public abstract class NetworkElement : LocatableObject, IDrawable, IDisposable
    {
        public abstract IReadOnlyKeyedList<string, NetworkPort> Ports { get; }
        public abstract IReadOnlyList<LanePath> Paths { get; }
        public abstract IReadOnlyList<LocatedModel> Models { get; }

        public NetworkElement(int plateX, int plateZ, Matrix4x4 transform) : base(plateX, plateZ, transform)
        {
        }

        public void Dispose()
        {
            foreach (LocatedModel model in Models) (model as CollidableLocatedModel)?.Dispose();
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
