using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace Bus.Common.Rendering
{
    public class Model : IModel
    {
        protected readonly IEnumerable<Mesh> Meshes;
        protected readonly IEnumerable<ID3D11ShaderResourceView> Textures;

        public Model(IEnumerable<Mesh> meshes, IEnumerable<ID3D11ShaderResourceView> textures)
        {
            Meshes = meshes;
            Textures = textures;
        }

        public static Model FromFile(ID3D11Device device, ID3D11DeviceContext context, string filePath)
        {
            AssimpModelFactory factory = new AssimpModelFactory(device, context);
            Model model = factory.FromFile(filePath);
            return model;
        }

        public void Dispose()
        {
            foreach (ID3D11ShaderResourceView texture in Textures)
            {
                texture.Dispose();
            }

            foreach (Mesh mesh in Meshes)
            {
                mesh.Dispose();
            }
        }

        public void Draw(ID3D11DeviceContext context)
        {
            foreach (Mesh mesh in Meshes)
            {
                mesh.Draw(context);
            }
        }
    }
}
