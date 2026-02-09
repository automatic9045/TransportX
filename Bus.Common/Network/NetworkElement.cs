using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Collections;
using Bus.Common.Rendering;
using Bus.Common.Scenery;

namespace Bus.Common.Network
{
    public abstract class NetworkElement : LocatableObject, IDrawable, IDisposable
    {
        public abstract IReadOnlyKeyedList<string, NetworkPort> Ports { get; }
        public abstract IReadOnlyList<ILanePath> Paths { get; }
        public abstract IReadOnlyList<LocatedModel> Models { get; }


        public string? DebugName
        {
            get => field;
            set
            {
                field = value;
                foreach (ILanePath path in Paths) path.DebugName = value;
            }
        } = null;

        public NetworkElement(int plateX, int plateZ, Pose pose) : base(plateX, plateZ, pose)
        {
        }

        public void Dispose()
        {
            foreach (LocatedModel model in Models) (model as CollidableLocatedModel)?.Dispose();
            foreach (ILanePath path in Paths) path.Dispose();
        }

        public void Draw(LocatedDrawContext context)
        {
            foreach (LocatedModel model in Models)
            {
                model.Draw(context);
            }

            if (context.Pass == RenderPass.Network)
            {
                foreach (ILanePath path in Paths)
                {
                    path.Draw(context);
                }
            }
        }
    }
}
