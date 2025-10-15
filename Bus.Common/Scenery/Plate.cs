using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using Bus.Common.Rendering;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scenery
{
    public class Plate : IDrawable
    {
        public static readonly int Size = 250;


        private bool IsFar = false;

        public List<LocatedModel> Models { get; } = new List<LocatedModel>();
        public List<NetworkElement> Network { get; } = new List<NetworkElement>();

        public Plate()
        {
        }

        public void ComputeTick(PlateOffset plateOffset)
        {
            bool isFar = 2 <= int.Abs(plateOffset.DeltaX) || 2 <= int.Abs(plateOffset.DeltaZ);
            if (IsFar && isFar) return;

            foreach (StaticLocatedModel model in Models)
            {
                model.UpdateColliderPose(plateOffset);
            }

            foreach (NetworkElement element in Network)
            {
                foreach (StaticLocatedModel model in element.Models)
                {
                    model.UpdateColliderPose(plateOffset);
                }
            }

            IsFar = isFar;
        }

        public void Draw(DrawContext context)
        {
            foreach (LocatedModel model in Models)
            {
                model.Draw(context);
            }

            foreach (NetworkElement element in Network)
            {
                element.Draw(context);
            }
        }
    }
}
