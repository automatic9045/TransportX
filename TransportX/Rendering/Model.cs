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
        public static Model Empty() => new([], [])
        {
            DebugName = "Empty",
        };


        private bool IsDisposed = false;

        public IReadOnlyList<IMesh> Meshes { get; }
        public IReadOnlyList<Material> Materials { get; }
        public BoundingBox BoundingBox { get; }

        public virtual string? DebugName
        {
            get => field;
            set
            {
                field = value;

                if (value is null)
                {
                    for (int i = 0; i < Meshes.Count; i++) Meshes[i].DebugName = Meshes[i].Name;
                    for (int i = 0; i < Materials.Count; i++) Materials[i].DebugName = Materials[i].Name;
                }
                else
                {

                    for (int i = 0; i < Meshes.Count; i++) Meshes[i].DebugName = $"{value}_{Meshes[i].Name}";
                    for (int i = 0; i < Materials.Count; i++) Materials[i].DebugName = $"{value}_{Materials[i].Name}";
                }
            }
        } = null;

        public Model(IReadOnlyList<IMesh> meshes, IReadOnlyList<Material> materials, BoundingBox boundingBox)
        {
            Meshes = meshes;
            Materials = materials;
            BoundingBox = boundingBox;

            for (int i = 0; i < Meshes.Count; i++)
            {
                if (!Materials.Contains(Meshes[i].Material))
                {
                    throw new ArgumentException($"メッシュ {i} ('{Meshes[i].Name}') の材質が {nameof(materials)} に含まれません。", nameof(materials));
                }
            }
        }

        public Model(IReadOnlyList<IMesh> meshes, IReadOnlyList<Material> materials) : this(meshes, materials, ComputeBoundingBox(meshes))
        {
        }

        private static BoundingBox ComputeBoundingBox(IReadOnlyList<IMesh> meshes)
        {
            BoundingBox boundingBox = meshes.Count == 0 ? default : meshes[0].BoundingBox;
            for (int i = 1; i < meshes.Count; i++)
            {
                boundingBox = BoundingBox.CreateMerged(boundingBox, meshes[i].BoundingBox);
            }
            return boundingBox;
        }

        public static ModelResourceSet Load(ID3D11DeviceContext context, IErrorCollector errorCollector, string path, bool makeLH)
        {
            using ModelFactory factory = new(context, null, errorCollector);
            ModelResourceSet model = factory.Load(path, makeLH);
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
