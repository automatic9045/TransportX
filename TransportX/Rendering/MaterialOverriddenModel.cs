using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Rendering
{
    public class MaterialOverriddenModel : IMeshModel
    {
        private IMesh[] MeshesCache;
        private Material[] RenderMaterials;

        public IMeshModel BaseModel { get; }
        public IReadOnlyDictionary<Material, Material> MaterialOverrides { get; }

        public BoundingBox BoundingBox => BaseModel.BoundingBox;
        public IReadOnlyList<IMesh> Meshes => BaseModel.Meshes;

        public string? DebugName { get; set; }

        public MaterialOverriddenModel(IMeshModel baseModel, IReadOnlyDictionary<Material, Material> materialOverrides)
        {
            BaseModel = baseModel;
            MaterialOverrides = materialOverrides;

            UpdateRenderMaterials();
        }

        public void Dispose()
        {
        }

        public void Draw(in DrawContext context)
        {
            if (!MeshesCache.SequenceEqual(BaseModel.Meshes)) UpdateRenderMaterials();

            for (int i = 0; i < MeshesCache.Length; i++)
            {
                MeshesCache[i].Draw(context, RenderMaterials[i]);
            }
        }

        [MemberNotNull(nameof(MeshesCache), nameof(RenderMaterials))]
        private void UpdateRenderMaterials()
        {
            int count = BaseModel.Meshes.Count;
            IMesh[] meshes = new IMesh[count];
            Material[] renderMaterials = new Material[count];

            for (int i = 0; i < count; i++)
            {
                meshes[i] = BaseModel.Meshes[i];

                MaterialOverrides.TryGetValue(meshes[i].Material, out Material? material);
                renderMaterials[i] = material ?? meshes[i].Material;
            }

            MeshesCache = meshes;
            RenderMaterials = renderMaterials;
        }
    }
}
