using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Importing
{
    internal readonly struct Model
    {
        public readonly Mesh[] Meshes { get; }
        public readonly Material[] Materials { get; }
        public readonly IReadOnlyDictionary<string, Texture> EmbeddedTextures { get; }

        public Model(Mesh[] meshes, Material[] materials, IReadOnlyDictionary<string, Texture> embeddedTextures)
        {
            Meshes = meshes;
            Materials = materials;
            EmbeddedTextures = embeddedTextures;
        }
    }
}
