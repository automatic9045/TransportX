using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Collections;
using TransportX.Components;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Network
{
    public abstract class NetworkElement : WorldObject, IDisposable
    {
        public abstract IReadOnlyKeyedList<string, NetworkPort> Ports { get; }
        public abstract IReadOnlyList<ILanePath> Paths { get; }
        public abstract IReadOnlyList<TransformedModel> Models { get; }
        public abstract IComponentCollection<IComponent> Components { get; }

        public string? DebugName
        {
            get => field;
            set
            {
                field = value;
                foreach (ILanePath path in Paths) path.DebugName = value;
            }
        } = null;

        public NetworkElement(WorldPose worldPose) : base(worldPose)
        {
        }

        public void Dispose()
        {
            foreach (TransformedModel model in Models) (model as CollidableTransformedModel)?.Dispose();
            foreach (ILanePath path in Paths) path.Dispose();
        }

        public void Draw<TCuller>(in TransformedDrawContext context, in TCuller culler) where TCuller : struct, ICullingVolume
        {
            foreach (TransformedModel model in Models)
            {
                Matrix4x4 world = (model.Pose * context.ChunkOffset.Pose).ToMatrix4x4();
                BoundingBox worldBox = BoundingBox.Transform(model.Model.BoundingBox, world);

                if (culler.Intersects(worldBox))
                {
                    model.Draw(context);
                }
            }

            if (context.Layer == RenderLayer.Network)
            {
                foreach (ILanePath path in Paths)
                {
                    path.Draw(context);
                }
            }
        }
    }
}
