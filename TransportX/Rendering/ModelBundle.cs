using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    public class ModelBundle : IModelBundle
    {
        public static ModelBundle Empty(string key) => new(key, new Dictionary<string, IModel>());


        public string Key { get; }
        public IReadOnlyDictionary<string, IModel> Models { get; }

        public ModelBundle(string key, IReadOnlyDictionary<string, IModel> models)
        {
            Key = key;
            Models = models;
        }

        public void Dispose()
        {
            foreach (IModel model in Models.Values)
            {
                model.Dispose();
            }
        }
    }
}
