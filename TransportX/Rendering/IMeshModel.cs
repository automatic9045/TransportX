using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    public interface IMeshModel : IModel
    {
        IReadOnlyList<IMesh> Meshes { get; }
        IReadOnlyList<Material> Materials { get; }

        public IMesh? FindMesh(string name)
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                IMesh mesh = Meshes[i];
                if (mesh.Name == name) return mesh;
            }

            return null;
        }

        public Material? FindMaterial(string name)
        {
            for (int i = 0; i < Materials.Count; i++)
            {
                Material material = Materials[i];
                if (material.Name == name) return material;
            }

            return null;
        }
    }
}
