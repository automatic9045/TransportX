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
        private IMesh[] Meshes;
        private Material[] RenderMaterials;

        public IMeshModel BaseModel { get; }
        public IReadOnlyDictionary<Material, Material> MaterialOverrides { get; }

        public BoundingBox BoundingBox => BaseModel.BoundingBox;
        public IReadOnlyList<IMesh> VisualMeshes => BaseModel.VisualMeshes;

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
            if (!Meshes.SequenceEqual(BaseModel.VisualMeshes)) UpdateRenderMaterials();

            for (int i = 0; i < Meshes.Length; i++)
            {
                Meshes[i].Draw(context, RenderMaterials[i]);
            }
        }

        [MemberNotNull(nameof(Meshes), nameof(RenderMaterials))]
        private void UpdateRenderMaterials()
        {
            int count = BaseModel.VisualMeshes.Count;
            IMesh[] meshes = new IMesh[count];
            Material[] renderMaterials = new Material[count];

            for (int i = 0; i < count; i++)
            {
                meshes[i] = BaseModel.VisualMeshes[i];

                MaterialOverrides.TryGetValue(meshes[i].Material, out Material? material);
                renderMaterials[i] = material ?? meshes[i].Material;
            }

            Meshes = meshes;
            RenderMaterials = renderMaterials;
        }
    }
}
