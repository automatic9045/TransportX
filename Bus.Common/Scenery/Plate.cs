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

        public void Update(PlateOffset fromCamera)
        {
            bool isFar = 2 <= int.Abs(fromCamera.DeltaX) || 2 <= int.Abs(fromCamera.DeltaZ);
            if (IsFar && isFar) return;

            foreach (LocatedModel model in Models)
            {
                if (model is CollidableLocatedModel collidableModel) collidableModel.Update(fromCamera);
            }

            foreach (NetworkElement element in Network)
            {
                foreach (LocatedModel model in element.Models)
                {
                    if (model is CollidableLocatedModel collidableModel) collidableModel.Update(fromCamera);
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
