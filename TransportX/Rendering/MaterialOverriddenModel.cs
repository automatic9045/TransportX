using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Rendering
{
    public class MaterialOverriddenModel : IMeshModel
    {
        public IMeshModel BaseModel { get; }
        public IReadOnlyDictionary<Material, Material> MaterialOverrides { get; }

        public IReadOnlyList<IMesh> Meshes { get; }
        public IReadOnlyList<Material> Materials { get; }
        public BoundingBox BoundingBox => BaseModel.BoundingBox;

        public string? DebugName { get; set; }

        public MaterialOverriddenModel(IMeshModel baseModel, IReadOnlyDictionary<Material, Material> materialOverrides)
        {
            BaseModel = baseModel;
            MaterialOverrides = materialOverrides;

            IMesh[] meshes = new IMesh[BaseModel.Meshes.Count];
            for (int i = 0; i < BaseModel.Meshes.Count; i++)
            {
                IMesh baseMesh = BaseModel.Meshes[i];
                MaterialOverrides.TryGetValue(baseMesh.Material, out Material? material);
                Material targetMaterial = material ?? baseMesh.Material;
                IMesh unwrappedBaseMesh = baseMesh is OverriddenMesh overridden ? overridden.BaseMesh : baseMesh;
                meshes[i] = new OverriddenMesh(unwrappedBaseMesh, targetMaterial);
            }
            Meshes = meshes;

            Material[] newMaterials = new Material[BaseModel.Materials.Count];
            for (int i = 0; i < BaseModel.Materials.Count; i++)
            {
                Material baseMaterial = BaseModel.Materials[i];
                MaterialOverrides.TryGetValue(baseMaterial, out Material? overriddenMaterial);
                newMaterials[i] = overriddenMaterial ?? baseMaterial;
            }
            Materials = newMaterials.Distinct().ToArray();
        }

        public void Dispose()
        {
        }

        public void Draw(in DrawContext context)
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].Draw(context);
            }
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
