using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Network;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class Plate : IDrawable, IDisposable
    {
        public static readonly int Size = 250;


        private bool IsFar = false;

        public int X { get; }
        public int Z { get; }
        public List<LocatedModel> Models { get; } = new List<LocatedModel>();
        public List<NetworkElement> Network { get; } = new List<NetworkElement>();

        public Plate(int x, int z)
        {
            X = x;
            Z = z;
        }

        public void Dispose()
        {
            foreach (LocatedModel model in Models) (model as CollidableLocatedModel)?.Dispose();
            foreach (NetworkElement element in Network) element.Dispose();
        }

        public void RegisterComponents(ComponentEngine engine)
        {
            foreach (NetworkElement element in Network)
            {
                engine.Register(element.Components);

                foreach (ILanePath lanePath in element.Paths)
                {
                    engine.Register(lanePath.Components);
                }
            }
        }

        public void SetFromCamera(PlateOffset fromCamera, int computePlateCount)
        {
            bool isFar = computePlateCount < int.Abs(fromCamera.DeltaX) + 1 || computePlateCount < int.Abs(fromCamera.DeltaZ) + 1;
            if (IsFar && isFar) return;

            foreach (LocatedModel model in Models)
            {
                if (model is CollidableLocatedModel collidableModel) collidableModel.SetFromCamera(fromCamera);
            }

            foreach (NetworkElement element in Network)
            {
                foreach (LocatedModel model in element.Models)
                {
                    if (model is CollidableLocatedModel collidableModel) collidableModel.SetFromCamera(fromCamera);
                }
            }

            IsFar = isFar;
        }

        public void Draw(in LocatedDrawContext context)
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
