using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Rendering;

namespace TransportX.Worlds
{
    public class ModelCollection : IModelCollection
    {
        private readonly Dictionary<string, IModel> Models = [];
        private readonly KeyedList<string, IModelBundle> BundlesKey = new(bundle => bundle.Key);

        public IReadOnlyKeyedList<string, IModelBundle> Bundles => BundlesKey;

        public ModelCollection()
        {
        }

        public void Dispose()
        {
            foreach (IModelBundle bundle in Bundles)
            {
                bundle.Dispose();
            }
        }

        public IModel GetModel(string modelKey)
        {
            return modelKey == string.Empty ? Model.Empty() : Models[modelKey];
        }

        public bool TryGetModel(string modelKey, [MaybeNullWhen(false)] out IModel model)
        {
            if (modelKey == string.Empty)
            {
                model = Model.Empty();
                return true;
            }
            else
            {
                return Models.TryGetValue(modelKey, out model);
            }
        }

        public bool AdoptBundle(IModelBundle bundle, bool allowOverride = false)
        {
            if (Bundles.TryGetValue(bundle.Key, out IModelBundle? existingBundle))
            {
                if (existingBundle == bundle)
                {
                    throw new ArgumentException("指定したモデルバンドルは既に登録されています。", nameof(bundle));
                }

                if (allowOverride)
                {
                    ReleaseBundle(bundle.Key);
                }
                else
                {
                    throw new ArgumentException($"キー '{bundle.Key}' のモデルバンドルは既に存在します。", nameof(bundle));
                }
            }

            BundlesKey.Add(bundle);

            foreach ((string modelKey, IModel model) in bundle.Models)
            {
                Models.Add(modelKey, model);
            }

            return true;
        }

        public bool ReleaseBundle(string bundleKey)
        {
            IModelBundle bundle = Bundles[bundleKey];

            foreach (string modelKey in bundle.Models.Keys)
            {
                Models.Remove(modelKey);
            }

            BundlesKey.Remove(bundleKey);
            bundle.Dispose();

            return true;
        }
    }
}
