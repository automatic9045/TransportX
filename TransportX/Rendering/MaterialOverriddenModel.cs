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
        private IMesh[] BaseMeshesCache;
        private IMesh[] MeshesCache;
        private Material[] MaterialsCache;

        public IMeshModel BaseModel { get; }
        public IReadOnlyDictionary<Material, Material> MaterialOverrides { get; }

        public IReadOnlyList<IMesh> Meshes => MeshesCache;
        public IReadOnlyList<Material> Materials => MaterialsCache;
        public BoundingBox BoundingBox => BaseModel.BoundingBox;

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
            if (!BaseMeshesCache.SequenceEqual(BaseModel.Meshes)) UpdateRenderMaterials();

            for (int i = 0; i < MeshesCache.Length; i++)
            {
                MeshesCache[i].Draw(context);
            }
        }

        [MemberNotNull(nameof(BaseMeshesCache), nameof(MeshesCache), nameof(MaterialsCache))]
        private void UpdateRenderMaterials()
        {
            IMesh[] baseMeshes = new IMesh[BaseModel.Meshes.Count];
            IMesh[] meshes = new IMesh[BaseModel.Meshes.Count];

            for (int i = 0; i < BaseModel.Meshes.Count; i++)
            {
                baseMeshes[i] = BaseModel.Meshes[i];
                MaterialOverrides.TryGetValue(baseMeshes[i].Material, out Material? material);

                meshes[i] = new OverriddenMesh(baseMeshes[i], material ?? baseMeshes[i].Material);
            }

            Material[] newMaterials = new Material[BaseModel.Materials.Count];
            for (int i = 0; i < BaseModel.Materials.Count; i++)
            {
                Material baseMaterial = BaseModel.Materials[i];
                MaterialOverrides.TryGetValue(baseMaterial, out Material? overriddenMaterial);
                newMaterials[i] = overriddenMaterial ?? baseMaterial;
            }

            BaseMeshesCache = baseMeshes;
            MeshesCache = meshes;
            MaterialsCache = newMaterials.Distinct().ToArray();
        }


        private class OverriddenMesh : IMesh
        {
            public IMesh BaseMesh { get; }

            public string Name => BaseMesh.Name;
            public BoundingBox BoundingBox => BaseMesh.BoundingBox;
            public Material Material { get; }

            public string? DebugName
            {
                get => BaseMesh.DebugName;
                set => BaseMesh.DebugName = value;
            }

            public OverriddenMesh(IMesh baseMesh, Material material)
            {
                BaseMesh = baseMesh;
                Material = material;
            }

            public void Dispose()
            {
            }

            public void Draw(in DrawContext context, Material renderMaterial) => BaseMesh.Draw(context, renderMaterial);
            public void Draw(in DrawContext context) => BaseMesh.Draw(context, Material);
        }
    }
}
