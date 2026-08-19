using System;
using System.Collections.Generic;
using System.Text;

namespace TransportX.Rendering
{
    public interface IMeshModel : IModel
    {
        IReadOnlyList<IMesh> Meshes { get; }

        public IMesh? FindMesh(string name)
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                IMesh mesh = Meshes[i];
                if (mesh.Name == name) return mesh;
            }

            return null;
        }
    }
}
