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


        public string? DebugName
        {
            get => field;
            set
            {
                field = value;
                foreach (LanePath path in Paths) path.DebugName = value;
            }
        } = null;

        public NetworkElement(int plateX, int plateZ, Pose pose) : base(plateX, plateZ, pose)
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

            if (context.DebugMode == DebugRenderingMode.Network)
            {
                foreach (LanePath path in Paths)
                {
                    if (path.CanDrawDebug) path.DrawDebug(context);
                }
            }
        }
    }
}
