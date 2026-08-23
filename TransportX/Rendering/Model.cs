using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Diagnostics;

namespace TransportX.Rendering
{
    public class Model : IMeshModel
    {
        public static Model Empty() => new([])
        {
            DebugName = "Empty",
        };


        private bool IsDisposed = false;

        public BoundingBox BoundingBox { get; }
        public IReadOnlyList<IMesh> Meshes { get; }

        public virtual string? DebugName
        {
            get => field;
            set
            {
                field = value;

                if (value is null)
                {
                    for (int i = 0; i < Meshes.Count; i++) Meshes[i].DebugName = null;
                }
                else
                {

                    for (int i = 0; i < Meshes.Count; i++) Meshes[i].DebugName = $"{value}_Material";
                }
            }
        } = null;

        public Model(IReadOnlyList<IMesh> visualMeshes, BoundingBox boundingBox)
        {
            Meshes = visualMeshes;
            BoundingBox = boundingBox;
        }

        public Model(IReadOnlyList<IMesh> visualMeshes) : this(visualMeshes, ComputeBoundingBox(visualMeshes))
        {
        }

        private static BoundingBox ComputeBoundingBox(IReadOnlyList<IMesh> visualMeshes)
        {
            BoundingBox boundingBox = visualMeshes.Count == 0 ? default : visualMeshes[0].BoundingBox;
            for (int i = 1; i < visualMeshes.Count; i++)
            {
                boundingBox = BoundingBox.CreateMerged(boundingBox, visualMeshes[i].BoundingBox);
            }
            return boundingBox;
        }

        public static ModelResourceSet Load(ID3D11DeviceContext context, IErrorCollector errorCollector, string visualModelPath, bool makeLH)
        {
            using ModelFactory factory = new(context, null, errorCollector);
            ModelResourceSet model = factory.Load(visualModelPath, makeLH);
            return model;
        }

        public virtual void Dispose()
        {
            if (IsDisposed) throw new InvalidOperationException();
            IsDisposed = true;

            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].Dispose();
            }
        }

        public void Draw(in DrawContext context)
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].Draw(context);
            }
        }
    }
}
