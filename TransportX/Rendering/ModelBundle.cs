using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public class ModelBundle : IModelBundle
    {
        public static ModelBundle Empty(string key) => new(key, new Dictionary<string, IModel>(), Enumerable.Empty<ID3D11ShaderResourceView>());


        protected readonly IEnumerable<ID3D11ShaderResourceView> Textures;

        public string Key { get; }
        public IReadOnlyDictionary<string, IModel> Models { get; }

        public ModelBundle(string key, IReadOnlyDictionary<string, IModel> models, IEnumerable<ID3D11ShaderResourceView> textures)
        {
            Key = key;
            Models = models;
            Textures = textures;
        }

        public void Dispose()
        {
            foreach (IModel model in Models.Values)
            {
                model.Dispose();
            }

            foreach (ID3D11ShaderResourceView texture in Textures)
            {
                texture.Dispose();
            }
        }
    }
}
