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
    public class Chunk : IDrawable, IDisposable
    {
        public static readonly int Size = 250;


        private bool IsFar = false;

        public int X { get; }
        public int Z { get; }
        public List<TransformedModel> Models { get; } = [];
        public List<NetworkElement> Network { get; } = [];

        public Chunk(int x, int z)
        {
            X = x;
            Z = z;
        }

        public void Dispose()
        {
            foreach (TransformedModel model in Models) (model as MergedKinematicTransformedModel)?.Dispose();
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

        public void SetFromCamera(ChunkOffset fromCamera, int computeChunkCount)
        {
            bool isFar = computeChunkCount < int.Abs(fromCamera.DeltaX) + 1 || computeChunkCount < int.Abs(fromCamera.DeltaZ) + 1;
            if (IsFar && isFar) return;

            foreach (TransformedModel model in Models)
            {
                if (model is CollidableTransformedModel collidableModel) collidableModel.SetFromCamera(fromCamera);
            }

            foreach (NetworkElement element in Network)
            {
                foreach (TransformedModel model in element.Models)
                {
                    if (model is CollidableTransformedModel collidableModel) collidableModel.SetFromCamera(fromCamera);
                }
            }

            IsFar = isFar;
        }

        public void Draw(in TransformedDrawContext context)
        {
            foreach (TransformedModel model in Models)
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
